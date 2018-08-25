#if NETCORE || NET45

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Evolve.Utilities;
using Microsoft.Extensions.DependencyModel;
using Microsoft.DotNet.PlatformAbstractions;

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
    ///         Moreover AppDomain is not available and replaced in NET Core by <see cref="AssemblyLoadContext"/>
    ///      </para>
    ///      <para>
    ///         This class rely on the dependency file ([appname].deps.json) of the .NET core application.
    ///         Once deserialized, this file gives us a <see cref="DependencyContext"/> where we can find
    ///         the path of the database driver assembly and its managed and native dependencies.
    ///         
    ///         Finally we load the driver and the managed assemblies with <see cref="AssemblyLoadContext.LoadFromAssemblyPath(string)"/>
    ///         And the native dependencies with <see cref="AssemblyLoadContext.LoadUnmanagedDllFromPath(string)"/>
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     This class is also used as a base class for .NET Standard/Core projects build by MSBuild. <see cref="CoreReflectionBasedDriverForNetEx"/>
    /// </remarks>
    public abstract class CoreReflectionBasedDriverEx : ReflectionBasedDriver
    {
        protected readonly string _depsFile;

#if NETCORE
        private readonly CustomAssemblyLoader _assemblyLoader;
#endif

        /// <summary>
        ///     Initializes a new instance of <see cref="CoreReflectionBasedDriverEx" /> with
        ///     the connection type name loaded from the specified assembly.
        /// </summary>
        /// <param name="driverAssemblyName"> Assembly to load the driver Type from. </param>
        /// <param name="connectionTypeName"> Name of the driver Type. </param>
        /// <param name="driverNugetPackageId"> Name of the Nuget package of the driver in the deps file. </param>
        /// <param name="depsFile"> Dependency file of the project to migrate. </param>
        /// <param name="nugetPackageDir"> Path to the NuGet package folder. </param>
        /// <exception cref="EvolveCoreDriverException"></exception>
        public CoreReflectionBasedDriverEx(string driverAssemblyName,
                                           string connectionTypeName,
                                           string driverNugetPackageId,
                                           string depsFile,
                                           string nugetPackageDir)
            : base(driverAssemblyName, connectionTypeName)
        {
            DriverNugetPackageId = driverNugetPackageId;
            _depsFile = Check.FileExists(depsFile, nameof(depsFile));
            NugetPackageDir = Check.DirectoryExists(nugetPackageDir, nameof(nugetPackageDir));
            ProjectDependencyContext = LoadDependencyContext(depsFile);
            try
            {
                NuGetFallbackDir = GetNuGetFallbackFolder();
            }
            catch (Exception ex)
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
        ///     NuGet package name of the driver in the deps file
        /// </summary>
        protected string DriverNugetPackageId { get; }

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
        ///     List of managed "compileOnly" libraries the driver assembly depends on.
        /// </summary>
        protected List<string> ManagedCompilationDependencies { get; set; } = new List<string>();

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
        /// <exception cref="EvolveCoreDriverException"></exception>
        protected override Type TypeFromAssembly()
        {
            ManagedDependencies = new List<string>();
            ManagedCompilationDependencies = new List<string>();
            NativeDependencies = new List<string>();

            var rootLib = GetLibrary(DriverNugetPackageId);
            FindDependencies(rootLib);

            string driverPath = GetDriverPath();

            ManagedDependencies.Where(x => !x.Equals(driverPath, StringComparison.OrdinalIgnoreCase))
                               .Where(x => !Path.GetFileName(x).Equals("System.Data.Common.dll", StringComparison.OrdinalIgnoreCase)) // .NET Core 1.1
                               .Where(x => !Path.GetFileName(x).Equals("sqlite3.dll", StringComparison.OrdinalIgnoreCase)) // .NET Core 1.1
                               .Distinct()
                               .ToList()
                               .ForEach(x => _assemblyLoader.LoadFromAssemblyPath(x));

            ManagedCompilationDependencies.Where(x => !x.Equals(driverPath, StringComparison.OrdinalIgnoreCase))
                                          .Distinct()
                                          .ToList()
                                          .ForEach(x => _assemblyLoader.LoadFromAssemblyPath(x));

            try
            {
                var driverAssembly = _assemblyLoader.LoadFromAssemblyPath(driverPath);
                return driverAssembly.GetType(DriverTypeName.Type);
            }
            catch (Exception ex)
            {
                throw new EvolveCoreDriverException($"Error loading driver assembly {DriverTypeName.Type} from {driverPath}", DumpDetails(), ex);
            }
        }
#endif

        /// <summary>
        ///     Traverse all the dependency tree of the given <paramref name="lib"/>,
        ///     searching for managed and native assembly files paths.
        /// </summary>
        /// <exception cref="EvolveCoreDriverException"></exception>
        protected void FindDependencies(Library lib)
        {
            if (lib == null)
            {
                return;
            }

            ManagedDependencies.AddRange(GetManagedAssembliesFullPath(lib));
            ManagedCompilationDependencies.AddRange(GetManagedCompilationAssembliesFullPath(lib));
            NativeDependencies.AddRange(GetNativeAssembliesFullPath(lib));

            foreach (var dependency in lib.Dependencies)
            {
                if (!ManagedDependencies.Any(x => Path.GetFileNameWithoutExtension(x).Equals(dependency.Name, StringComparison.OrdinalIgnoreCase)) &&
                    !ManagedCompilationDependencies.Any(x => Path.GetFileNameWithoutExtension(x).Equals(dependency.Name, StringComparison.OrdinalIgnoreCase)) &&
                    !NativeDependencies.Any(x => Path.GetFileNameWithoutExtension(x).Equals(dependency.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    var depLib = GetLibrary(dependency.Name);
                    FindDependencies(depLib); // rec
                }
            }
        }

        /// <summary>
        ///     Returns the list of managed assemblies of a given <paramref name="lib"/>.
        /// </summary>
        /// <exception cref="EvolveCoreDriverException"></exception>
        protected virtual List<string> GetManagedAssembliesFullPath(Library lib)
        {
            Check.NotNull(lib, nameof(lib));

            var runtimeLib = lib as RuntimeLibrary;
            if (runtimeLib is null)
            {
                return new List<string>();
            }

            if (runtimeLib.RuntimeAssemblyGroups == null || runtimeLib.RuntimeAssemblyGroups.Count == 0)
            {
                return new List<string>();
            }

            var paths = new List<string>();
            string packageDir = GetPackageFolder(runtimeLib);
            foreach (var assemblyAssetGroup in runtimeLib.RuntimeAssemblyGroups.SelectMany(x => x.AssetPaths.Select(y => new { x.Runtime, Path = y }))
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
        ///     Returns the list of managed compilation assemblies of a given <paramref name="lib"/>.
        /// </summary>
        protected virtual List<string> GetManagedCompilationAssembliesFullPath(Library lib)
        {
            Check.NotNull(lib, nameof(lib));

            var emptyResult = new List<string>();
            var compilationLib = lib as CompilationLibrary;
            if (compilationLib is null || compilationLib.Assemblies.Count == 0)
            {
                return emptyResult;
            }

            string basePath = Path.Combine(NuGetFallbackDir, compilationLib.Path);
            if (Directory.Exists(Path.Combine(basePath, "runtimes")))
            {
                basePath = Path.Combine(basePath, "runtimes");
#if NETCORE
                string platform = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                    ? "win"
                    : "unix";
                basePath = Path.Combine(basePath, platform);
#else
                basePath = Path.Combine(basePath, "win");
#endif
                if (!Directory.Exists(basePath))
                {
                    return emptyResult;
                }
            }
            string libPath = Path.Combine(basePath, compilationLib.Assemblies[0].Replace("ref/", "lib/"));
            if (!File.Exists(libPath))
            { // Hack for SqlServer dependencies: System.Security.AccessControl and System.Security.Principal.Windows
              // that points to unknown netstandard folders in the runtimes directory
                libPath = libPath.Replace("netstandard", "netcoreapp");
                if (!File.Exists(libPath))
                {
                    return emptyResult;
                }
            }

            return new List<string> { libPath };
        }

        /// <summary>
        ///     Returns the list of native libraries of a given <paramref name="lib"/>,
        ///     depending the os and the process architecture the application is running.
        /// </summary>
        /// <exception cref="EvolveCoreDriverException"></exception>
        protected virtual List<string> GetNativeAssembliesFullPath(Library lib)
        {
            Check.NotNull(lib, nameof(lib));

            RuntimeLibrary runtimeLib = lib as RuntimeLibrary;
            if (runtimeLib is null)
            {
                return new List<string>();
            }

            if (runtimeLib.NativeLibraryGroups == null || runtimeLib.NativeLibraryGroups.Count == 0)
            {
                return new List<string>();
            }

            var paths = new List<string>();
            string packageDir = GetPackageFolder(runtimeLib);

            paths.AddRange(runtimeLib.NativeLibraryGroups.Where(x => IsRuntimeCompatible(x.Runtime))
                                                         .SelectMany(x => x.AssetPaths)
                                                         .Select(x => Path.Combine(packageDir, x)));

            return paths;
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
        /// <exception cref="EvolveCoreDriverException">PackageFolderNotFound</exception>
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

            throw new EvolveCoreDriverException($"Package folder {lib.Name} not found. Searched location 1: {path1} - Searched location 2: {path2}", DumpDetails());
        }

        private string GetDriverPath()
        {
            try
            {
                return ManagedDependencies.FirstOrDefault(x => x.Contains(DriverTypeName.Assembly)) ??
                       ManagedCompilationDependencies.First(x => x.Contains(DriverTypeName.Assembly));
            }
            catch
            {
                throw new EvolveCoreDriverException($"Assembly {DriverTypeName.Assembly} not found.", DumpDetails());
            }
        }

        /// <summary>
        ///     Returns a library from the deps file, given an <paramref name="assemblyName"/>.
        /// </summary>
        /// <param name="assemblyName"> The name of the assembly to find in the deps file. </param>
        /// <returns> The runtime or the compilation library. </returns>
        /// <exception cref="EvolveCoreDriverException">
        ///     Throws an EvolveCoreDriverException when the given assembly name is not found in deps file.
        /// </exception>
        protected Library GetLibrary(string assemblyName)
        {
            try
            {
                return ProjectDependencyContext.RuntimeLibraries.SingleOrDefault(x => x.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)) ??
                       ProjectDependencyContext.CompileLibraries.Single(x => x.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)) as Library;
            }
            catch (Exception ex)
            {
                throw new EvolveCoreDriverException($"Failed to load assembly {assemblyName} from deps file at {_depsFile}.", DumpDetails(), ex);
            }
        }

        protected string DumpDetails()
        {
            try
            {
                return
                $"Driver details: " + Environment.NewLine +
                $"- Assembly: {DriverTypeName.Assembly}" + Environment.NewLine +
                $"- Type: {DriverTypeName.Type}" + Environment.NewLine +
                $"- OS platform: {String.Join(", ", OSPlatform)}" + Environment.NewLine +
                $"- Process architecture: {ProcessArchitecture}" + Environment.NewLine +
                $"- Deps file: {_depsFile}" + Environment.NewLine +
                $"- Managed dependencies found: " + Environment.NewLine +
                $"{String.Join(Environment.NewLine, ManagedDependencies.Select(x => "    " + x))}" + Environment.NewLine +
                $"- Native dependencies found: " + Environment.NewLine +
                $"{String.Join(Environment.NewLine, NativeDependencies.Select(x => "    " + x))}" + Environment.NewLine + Environment.NewLine;
            }
            catch
            {
                return "Driver details: Error retrieving driver details." + Environment.NewLine + Environment.NewLine;
            }
        }

        /// <summary>
        ///     Load the dependency context of the targeted application from a deps.json file.
        /// </summary>
        /// <param name="depsFile"> Path to the deps.json file. </param>
        /// <returns> A dependency context. </returns>
        /// <exception cref="EvolveCoreDriverException"> Throws an EvolveException when the loading fails. </exception>
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
                throw new EvolveCoreDriverException($"Failed to load dependency context from {depsFile}.", ex);
            }
        }

        /// <summary>
        ///     Returns the NuGetFallbackFolder path whereas the process is .NET or .NET Core
        /// </summary>
        /// <exception cref="EvolveCoreDriverException"> NuGetFallbackFolder not found. </exception>
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

            throw new EvolveCoreDriverException("NuGetFallbackFolder not found.");
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
                if (assemblyName.Name == "System.Text.Encoding.CodePages")
                { // Hack netcoreapp21 for SqlServer
                    var lib = _driverLoader.ProjectDependencyContext.CompileLibraries.FirstOrDefault(x => x.Name == "System.Text.Encoding.CodePages");
                    string basePath = Path.Combine(_driverLoader.NuGetFallbackDir, lib.Path);
                    return context.LoadFromAssemblyPath(Path.Combine(basePath, "runtimes/win/lib/netstandard2.0/System.Text.Encoding.CodePages.dll"));
                }

                return null;
            }

            protected override Assembly Load(AssemblyName assemblyName) => null;

            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                string unmanagedDllNameWithoutExt = Path.GetFileNameWithoutExtension(unmanagedDllName); // clean the name
                string unmanagedDllPath = _driverLoader.NativeDependencies.SingleOrDefault(x => Path.GetFileNameWithoutExtension(x).Contains(unmanagedDllNameWithoutExt, StringComparison.OrdinalIgnoreCase));
                if (unmanagedDllPath != null)
                {
                    try
                    {
                        return LoadUnmanagedDllFromPath(unmanagedDllPath);
                    }
                    catch (Exception ex)
                    {
                        throw new EvolveCoreDriverException($"Error loading native assembly at {unmanagedDllPath}.", _driverLoader.DumpDetails(), ex);
                    }
                }
                
                if (unmanagedDllName == "sni.dll")
                { // Hack netcoreapp21 for SqlServer
                    string sniLibSuffix = ".runtime.native.System.Data.SqlClient.sni";
                    foreach (var lib in _driverLoader.ProjectDependencyContext.CompileLibraries.Where(x => x.Name.Contains(sniLibSuffix)))
                    {
                        string rid = lib.Name.Replace(sniLibSuffix, "").Replace("runtime.", "");
                        if (_driverLoader.IsRuntimeCompatible(rid))
                        {
                            string basePath = Path.Combine(_driverLoader.NuGetFallbackDir, lib.Path);
                            unmanagedDllPath = Directory.GetFiles(basePath, "sni.dll", SearchOption.AllDirectories).FirstOrDefault();
                            try
                            {
                                return LoadUnmanagedDllFromPath(unmanagedDllPath);
                            }
                            catch (Exception ex)
                            {
                                throw new EvolveCoreDriverException($"Error loading native assembly at {unmanagedDllPath}.", _driverLoader.DumpDetails(), ex);
                            }
                        }
                    }
                }

                try
                {
                    return base.LoadUnmanagedDll(unmanagedDllName);
                }
                catch (Exception ex)
                {
                    throw new EvolveCoreDriverException($"Native assembly {unmanagedDllName} not found.", _driverLoader.DumpDetails(), ex);
                }
            }
        }

#endif

    }
}

#endif