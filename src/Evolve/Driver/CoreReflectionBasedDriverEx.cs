#if NETCORE || NET45

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Evolve.Utilities;

#if NETCORE
using System.Runtime.InteropServices;
using System.Runtime.Loader;
#endif

namespace Evolve.Driver
{
    /// <summary>
    ///     <para>
    ///         Base class for drivers loaded by reflection from a .NET Standard/Core project via a dotnet-build command.
    ///     </para>
    ///     <para>
    ///         The loading strategy differs from a .NET project because all
    ///         the assemblies needed by the driver are not in the application build folder.
    ///         
    ///         Moreover AppDomain is not available and replaced in NETCORE by <see cref="AssemblyLoadContext"/>
    ///      </para>
    ///      <para>
    ///         This class rely on the dependency file ([appname].deps.json) of the .NET core application.
    ///         Once deserialized, this file gives us a <see cref="DependencyContext"/> where we can find
    ///         the path of the database driver assembly and its managed and native dependencies.
    ///         
    ///         Finally we load the driver with <see cref="AssemblyLoadContext.LoadFromAssemblyPath(string)"/>
    ///         And the native dependencies with <see cref="AssemblyLoadContext.LoadUnmanagedDllFromPath(string)"/>
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     This class is also used as a base class for .NET Standard/Core projects build by MSBuild. <see cref="CoreReflectionBasedDriverForNet"/>
    /// </remarks>
    public abstract class CoreReflectionBasedDriverEx : ReflectionBasedDriver
    {
        private readonly string _depsFile;

#if NETCORE
        private readonly CustomAssemblyLoader _assemblyLoader;
#endif

        /// <summary>
        ///     Initializes a new instance of <see cref="CoreReflectionBasedDriver" /> with
        ///     the connection type name loaded from the specified assembly.
        /// </summary>
        /// <param name="driverAssemblyName"> Assembly to load the driver Type from. </param>
        /// <param name="connectionTypeName"> Name of the driver Type. </param>
        /// <param name="depsFile"> Dependency file of the project to migrate. </param>
        /// <param name="nugetPackageDir"> Path to the NuGet package folder. </param>
        public CoreReflectionBasedDriverEx(string driverAssemblyName, string connectionTypeName, string depsFile, string nugetPackageDir) : base(driverAssemblyName, connectionTypeName)
        {
            _depsFile = Check.FileExists(depsFile, nameof(depsFile));
            NugetPackageDir = Check.DirectoryExists(nugetPackageDir, nameof(nugetPackageDir));
            ProjectDependencyContext = LoadDependencyContext(depsFile);
            try
            {
                NuGetFallbackDir = GetNuGetFallbackFolder();
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"NuGetFallbackFolder not found. {ex.Message}");
            }

#if NETCORE
            _assemblyLoader = new CustomAssemblyLoader(this);
#endif
        }

        /// <summary>
        ///     A dependency context loaded from the application <see cref="_depsFile"/>
        /// </summary>
        protected DependencyContext ProjectDependencyContext { get; }

        /// <summary>
        ///     NuGet package cache folder
        /// </summary>
        protected string NugetPackageDir { get; }

        /// <summary>
        ///     NuGet package fallback folder
        /// </summary>
        protected string NuGetFallbackDir { get; }

        /// <summary>
        ///     List of native libraries the driver assembly depends on.
        /// </summary>
        protected List<string> NativeDependencies { get; set; } = new List<string>();

        /// <summary>
        ///     List of managed libraries the driver assembly depends on.
        /// </summary>
        protected List<string> ManagedDependencies { get; set; } = new List<string>();

        /// <summary>
        ///     Returns whether the application is a x64 or x86 or arm or arm64 process.
        /// </summary>
        protected string ProcessArchitecture =>
#if NETCORE
            RuntimeInformation.ProcessArchitecture.ToString();
#else
            Environment.Is64BitProcess ? "x64" : "x86";
#endif

        /// <summary>
        ///     Determines the operating system on which the application is running.
        ///     (win or linux or unix or osx or unix)
        /// </summary>
        /// <exception cref="PlatformNotSupportedException"> 
        ///     Throws a PlatformNotSupportedException when the the os is neither based on Windows, Linux or OSX. 
        /// </exception>
        protected IEnumerable<string> OSPlatform
        {
            get
            {
#if NETCORE
                if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    return new List<string> { "win" };
                }
                else if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    return new List<string> { "linux", "unix" };
                }
                else if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                    return new List<string> { "osx", "unix" };
                }

                throw new PlatformNotSupportedException();
#else
            return new List<string> { "win" };
#endif
            }
        }

#if NETCORE
        /// <summary>
        ///     Load the driver <see cref="Type"/> from a .deps file definition.
        /// </summary>
        /// <returns> The driver type. </returns>
        protected override Type TypeFromAssembly()
        {
            ManagedDependencies = new List<string>();
            NativeDependencies = new List<string>();

            RuntimeLibrary rootLib = GetRuntimeLibrary(DriverTypeName.Assembly);
            FindDependencies(rootLib);

            string driverPath = ManagedDependencies.FirstOrDefault(x => x.Contains(DriverTypeName.Assembly));
            if (string.IsNullOrEmpty(driverPath))
            {
                throw new EvolveException($"Assembly {DriverTypeName.Assembly} not found.");
            }

            ManagedDependencies.Where(x => !x.Equals(driverPath, StringComparison.OrdinalIgnoreCase))
                               .Where(x => !Path.GetFileName(x).Equals("System.Data.Common.dll", StringComparison.OrdinalIgnoreCase)) // .NET Core 1.1
                               .Distinct()
                               .ToList()
                               .ForEach(x => _assemblyLoader.LoadFromAssemblyPath(x));

            var driverAssembly = _assemblyLoader.LoadFromAssemblyPath(driverPath);
            return driverAssembly.GetType(DriverTypeName.Type);
        }
#endif

        /// <summary>
        ///     Returns the list of managed assemblies of a given <paramref name="lib"/>.
        /// </summary>
        protected virtual List<string> GetManagedAssembliesFullPath(RuntimeLibrary lib)
        {
            Check.NotNull(lib, nameof(lib));

            if (lib.RuntimeAssemblyGroups == null || lib.RuntimeAssemblyGroups.Count == 0)
            {
                return new List<string>();
            }

            var paths = new List<string>();
            string packageDir = GetPackageFolder(lib);
            foreach (var assemblyAssetGroup in lib.RuntimeAssemblyGroups.SelectMany(x => x.AssetPaths.Select(y => new { x.Runtime, Path = y }))
                                                  .GroupBy(x => Path.GetFileName(x.Path)))
            {
                paths.Add(Path.Combine(packageDir, assemblyAssetGroup.Where(x => x.Runtime == "" || IsRuntimeCompatible(x.Runtime))
                                                                     .OrderByDescending(x => x.Runtime)
                                                                     .First()
                                                                     .Path));
            }

            return paths;
        }

        /// <summary>
        ///     Returns the list of native libraries of a given <paramref name="lib"/>,
        ///     depending the os and the process architecture the application is running.
        /// </summary>
        protected virtual List<string> GetNativeAssembliesFullPath(RuntimeLibrary lib)
        {
            Check.NotNull(lib, nameof(lib));

            if (lib.NativeLibraryGroups == null || lib.NativeLibraryGroups.Count == 0)
            {
                return new List<string>();
            }

            var paths = new List<string>();
            string packageDir = GetPackageFolder(lib);

            paths.AddRange(lib.NativeLibraryGroups.Where(x => IsRuntimeCompatible(x.Runtime))
                                                  .SelectMany(x => x.AssetPaths)
                                                  .Select(x => Path.Combine(packageDir, x)));

            return paths;
        }

        /// <summary>
        ///     Traverse all the dependency tree of the given <paramref name="lib"/>,
        ///     searching for managed and native assembly files paths.
        /// </summary>
        protected void FindDependencies(RuntimeLibrary lib)
        {
            if (lib == null)
            {
                return;
            }

            ManagedDependencies.AddRange(GetManagedAssembliesFullPath(lib));
            NativeDependencies.AddRange(GetNativeAssembliesFullPath(lib));

            foreach (var dependency in lib.Dependencies)
            {
                RuntimeLibrary depLib = GetRuntimeLibrary(dependency.Name);
                if (depLib == null)
                {
                    continue; // it's a "compileOnly" assembly
                }

                if (!ManagedDependencies.Any(x => Path.GetFileNameWithoutExtension(x).Equals(dependency.Name, StringComparison.OrdinalIgnoreCase)) &&
                    !NativeDependencies.Any(x => Path.GetFileNameWithoutExtension(x).Equals(dependency.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    FindDependencies(depLib); // rec
                }
            }
        }

        /// <summary>
        ///     Returns true if the RID is compatible with this os plateform, false otherwise.
        /// </summary>
        private bool IsRuntimeCompatible(string runtime)
        {
            var rid = new RID(runtime);
            if (!OSPlatform.Any(os => rid.OS.Contains(os, StringComparison.OrdinalIgnoreCase)))
            {
                return false; // "The package is not meant for this operation system
            }

            if (string.IsNullOrEmpty(rid.Architecture))
            {
                return true;
            }
            else
            {
                if (rid.Architecture.Equals(ProcessArchitecture, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false; // "The package is not meant for this architecture
                }
            }
        }

        /// <summary>
        ///     Returns the path of a given <paramref name="lib"/> on the disk,
        ///     whereas its located in the <see cref="NugetPackageDir"/> or the <see cref="NuGetFallbackDir"/>.
        /// </summary>
        /// <exception cref="EvolveException">PackageFolderNotFound</exception>
        private string GetPackageFolder(RuntimeLibrary lib)
        {
            string path1 = Path.Combine(NugetPackageDir, lib.Path);
            if (Directory.Exists(path1))
            {
                return path1;
            }

            string path2 = Path.Combine(NuGetFallbackDir, lib.Path);
            if (Directory.Exists(path2))
            {
                return path2;
            }

            throw new EvolveException($"Package folder {lib.Name} not found. Searched location 1: {path1} - Searched location 2: {path2}");
        }

        private bool IsLibraryNative(RuntimeLibrary lib) => lib?.NativeLibraryGroups.Count > 0;

        /// <summary>
        ///     Return a list of dependencies, as well as compilation context data and compilation dependencies 
        ///     found in the deps file for a given assembly.
        /// </summary>
        /// <param name="assemblyName"> The name of the assembly to find in the deps file. </param>
        /// <returns> The runtime library or null if its a "compileOnly" assembly. </returns>
        /// <exception cref="EvolveException"> Throws an EvolveException when data of the given assembly name is not found in deps file. </exception>
        protected RuntimeLibrary GetRuntimeLibrary(string assemblyName)
        {
            RuntimeLibrary lib = ProjectDependencyContext.RuntimeLibraries.SingleOrDefault(x => x.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
            if (lib != null)
            {
                return lib;
            }
            else
            {
                try
                {
                    ProjectDependencyContext.CompileLibraries.Single(x => x.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
                    return null;
                }
                catch (Exception ex)
                {
                    throw new EvolveException($"Failed to load assembly {assemblyName} from deps file at {_depsFile}.", ex);
                }
            }
        }

        /// <summary>
        ///     Load the dependency context of the application to evolve from a deps.json file.
        /// </summary>
        /// <param name="depsFile"> Path to the deps.json file. </param>
        /// <returns> A dependency context. </returns>
        /// <exception cref="EvolveException"> Throws an EvolveException when the loading fails. </exception>
        private static DependencyContext LoadDependencyContext(string depsFile)
        {
            try
            {
                using (var reader = new DependencyContextJsonReader())
                {
                    using (var stream = File.OpenRead(depsFile))
                    {
                        return reader.Read(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EvolveException($"Failed to load dependency context from {depsFile}.", ex);
            }
        }

        /// <summary>
        ///     Returns the NuGetFallbackFolder path whereas the process is .NET or .NET Core
        /// </summary>
        /// <exception cref="EvolveException"> NuGetFallbackFolder not found. </exception>
        private static string GetNuGetFallbackFolder()
        {
            string fallbackDir = "";

#if NETCORE
            fallbackDir = Path.Combine(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(GC).GetTypeInfo().Assembly.Location), @"../../..")), "sdk/NuGetFallbackFolder");
            if (Directory.Exists(fallbackDir))
            {
                return fallbackDir;
            }
#endif
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--info",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("Base Path:", StringComparison.OrdinalIgnoreCase)) //  Base Path:   C:\Program Files\dotnet\sdk\2.1.4\
                {
                    fallbackDir = Path.Combine(Path.GetFullPath(Path.Combine(line.Replace("Base Path:", "").Trim(), @"..")), "NuGetFallbackFolder");
                }
            }

            if (Directory.Exists(fallbackDir))
            {
                return fallbackDir;
            }

            throw new EvolveException("NuGetFallbackFolder not found.");
        }

#if NETCORE

        /// <summary>
        ///     Class responsible for dynamic load of assemblies in .NET Core
        /// </summary>
        private class CustomAssemblyLoader : AssemblyLoadContext
        {
            private readonly CoreReflectionBasedDriverEx _driverLoader;

            public CustomAssemblyLoader(CoreReflectionBasedDriverEx driverLoader)
            {
                _driverLoader = driverLoader;

                Resolving += CustomAssemblyLoader_Resolving;
            }

            private Assembly CustomAssemblyLoader_Resolving(AssemblyLoadContext context, AssemblyName assemblyName)
            {
                RuntimeLibrary lib = _driverLoader.GetRuntimeLibrary(assemblyName.Name);
                var assemblies = _driverLoader.GetManagedAssembliesFullPath(lib);
                if (assemblies.Count == 0)
                {
                    throw new EvolveException($"{assemblyName} not found.");
                }

                return context.LoadFromAssemblyPath(assemblies[0]);
            }

            protected override Assembly Load(AssemblyName assemblyName) => null;

            /// <summary>
            ///     Load an unmanaged dependency where the path was previously stored in <see cref="CoreReflectionBasedDriver.NativeDependencies"/>
            /// </summary>
            /// <param name="unmanagedDllName"> The name of the dll to load. </param>
            /// <returns></returns>
            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                string unmanagedDllNameWithoutExt = Path.GetFileNameWithoutExtension(unmanagedDllName); // clean the name
                string unmanagedDllPath = _driverLoader.NativeDependencies.Single(x => Path.GetFileNameWithoutExtension(x).Contains(unmanagedDllNameWithoutExt, StringComparison.OrdinalIgnoreCase));
                if (unmanagedDllPath != null)
                {
                    return LoadUnmanagedDllFromPath(unmanagedDllPath);
                }

                return base.LoadUnmanagedDll(unmanagedDllName);
            }
        }

#endif

    }
}

#endif