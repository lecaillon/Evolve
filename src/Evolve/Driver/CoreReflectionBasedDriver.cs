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
    public abstract class CoreReflectionBasedDriver : ReflectionBasedDriver
    {
        private const string RuntimeLibraryLoadingError = "Failed to load assembly {0} from deps file at {1}.";
        private const string DependencyContextLoadingError = "Failed to load dependency context from {0}.";
        private const string NoRuntimeTargetsFound = "No <runtimeTargets> found in the deps file for the assembly: {0}.";
        private const string NoRuntimeTargetsFoundForOS = "None of the <runtimeTargets> matches the corresponding os: {0}, for the assembly: {1}.";
        private const string NoRuntimeTargetsFoundForOSArchitecture = "None of the <runtimeTargets> matches the corresponding os: {0} and os architecture: {1}, for the assembly: {2}.";
        private const string MultipleRuntimeTargetsFound = "Evolve can not define the correct assembly for your system: {0}";
        private const string NuGetFallbackDirNotFound = "NuGetFallbackFolder not found. {0}";
        private const string PackageNotFound = "Package {0} not found. Searched location 1: {1} - Searched location 2: {2}";

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
        public CoreReflectionBasedDriver(string driverAssemblyName, string connectionTypeName, string depsFile, string nugetPackageDir) : base(driverAssemblyName, connectionTypeName)
        {
            _depsFile = Check.FileExists(depsFile, nameof(depsFile));
            NugetPackageDir = Check.DirectoryExists(nugetPackageDir, nameof(nugetPackageDir));
            ProjectDependencyContext = LoadDependencyContext(_depsFile);
            NativeDependencies = new List<string>();
            try
            {
                NuGetFallbackDir = Path.Combine(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(
                    typeof(GC).GetTypeInfo().Assembly.Location), @"../../..")), "sdk/NuGetFallbackFolder");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(string.Format(NuGetFallbackDirNotFound, ex.Message));
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
        protected List<string> NativeDependencies { get; set; }

#if NETCORE
        /// <summary>
        ///     Load the driver <see cref="Type"/> from a .deps file definition.
        /// </summary>
        /// <returns> The driver type. </returns>
        protected override Type TypeFromAssembly()
        {
            RuntimeLibrary lib = GetRuntimeLibrary(DriverTypeName.Assembly);

            StoreDriverNativeDependencies(lib);

            if (lib.Name == "Microsoft.Data.Sqlite" && lib.Version[0] == '2')
            {
                string extraPath2 = GetAssemblyPath(GetRuntimeLibrary("SQLitePCLRaw.provider.e_sqlite3.netstandard11"));
                _assemblyLoader.LoadFromAssemblyPath(extraPath2);

                _assemblyLoader.LoadFromAssemblyPath(@"C:/Program Files/dotnet/sdk/NuGetFallbackFolder/SQLitePCLRaw.bundle_green/1.1.7/lib/netcoreapp/SQLitePCLRaw.batteries_green.dll");
                _assemblyLoader.LoadFromAssemblyPath(@"C:/Program Files/dotnet/sdk/NuGetFallbackFolder/SQLitePCLRaw.bundle_green/1.1.7/lib/netcoreapp/SQLitePCLRaw.batteries_v2.dll");

                lib = GetRuntimeLibrary("Microsoft.Data.Sqlite.Core");
            }
            string driverPath = GetAssemblyPath(lib);
            var driverAssembly = _assemblyLoader.LoadFromAssemblyPath(driverPath);
            return driverAssembly.GetType(DriverTypeName.Type);
        }
#endif

        /// <summary>
        ///     Find and store native driver dependencies for later loading.
        /// </summary>
        /// <param name="lib"> The driver runtime library. </param>
        protected void StoreDriverNativeDependencies(RuntimeLibrary lib)
        {
            foreach (var dependency in lib.Dependencies)
            {
                RuntimeLibrary depLib = GetRuntimeLibrary(dependency.Name);
                if (depLib == null)
                {
                    continue; // it's a "compileOnly" assembly
                }

                if (IsLibraryNative(depLib) && !NativeDependencies.Contains(dependency.Name))
                {
                    string path = GetNativeLibraryPath(depLib);
                    if (path != null)
                    {
                        NativeDependencies.Add(path);
                    }
                }

                StoreDriverNativeDependencies(depLib); // rec
            }
        }

        /// <summary>
        ///     Define the path of a managed assembly given the os and the process architecture the application is running.
        /// </summary>
        /// <param name="lib"> A resource assembly in the <see cref="_depsFile"/> </param>
        /// <returns> The assembly full path, or null if not found. </returns>
        protected virtual string GetAssemblyPath(RuntimeLibrary lib)
        {
            string path = GetAssemblyRelativePath(lib);
            return path == null ? null : Path.Combine(GetLibraryPackageFolderPath(lib), path);
        }

        /// <summary>
        ///     Define the path of a native library given the os and the process architecture the application is running.
        /// </summary>
        /// <param name="lib"> A resource library in the <see cref="_depsFile"/> </param>
        /// <returns> The library full path, or null if not found. </returns>
        private string GetNativeLibraryPath(RuntimeLibrary lib)
        {
            string path = GetNativeAssemblyRelativePath(lib);
            return path == null ? null : Path.Combine(GetLibraryPackageFolderPath(lib), path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lib"></param>
        /// <returns></returns>
        private string GetLibraryPackageFolderPath(RuntimeLibrary lib)
        {
            if (Directory.Exists(Path.Combine(NugetPackageDir, lib.Path)))
            {
                return Path.Combine(NugetPackageDir, lib.Path);
            }

            if (Directory.Exists(Path.Combine(NuGetFallbackDir, lib.Path)))
            {
                return Path.Combine(NuGetFallbackDir, lib.Path);
            }

            throw new EvolveException(string.Format(PackageNotFound, lib.Name, 
                                                                     Path.Combine(NugetPackageDir, lib.Path), 
                                                                     Path.Combine(NuGetFallbackDir, lib.Path)));
        }

        private string GetAssemblyRelativePath(RuntimeLibrary lib) => GetRuntimeAssemblyAssetGroup(lib.Name, lib.RuntimeAssemblyGroups)?.AssetPaths[0];

        private string GetNativeAssemblyRelativePath(RuntimeLibrary lib) => GetRuntimeAssemblyAssetGroup(lib.Name, lib.NativeLibraryGroups)?.AssetPaths[0];

        private bool IsLibraryNative(RuntimeLibrary lib) => lib?.NativeLibraryGroups.Count > 0;

        /// <summary>
        ///     <para>
        ///         Given a list of assembly, define the one that targets the os and the process architecture the application is running.
        ///     </para>
        ///     <para>
        ///         I am aware of the naivety of the approach, but so far it works for the few database drivers Evolve offers.
        ///     </para>
        /// </summary>
        /// <param name="assemblyName"> Name of the assembly. </param>
        /// <param name="runtimeAssetGroups"> A list of assembly and their RID. </param>
        /// <returns> The relative path to the selected assembly, or null if none found. </returns>
        private RuntimeAssetGroup GetRuntimeAssemblyAssetGroup(string assemblyName, IReadOnlyList<RuntimeAssetGroup> runtimeAssetGroups)
        {
            Check.NotNullOrEmpty(assemblyName, nameof(assemblyName));
            Check.NotNull(runtimeAssetGroups, nameof(runtimeAssetGroups));

            if (runtimeAssetGroups.Count == 0)
            {
                Debug.WriteLine(string.Format(NoRuntimeTargetsFound, assemblyName));
                return null;
            }

            // Only one path to a cross platform assembly, choice is made !
            if (runtimeAssetGroups.Count == 1 && runtimeAssetGroups[0].Runtime == "")
            {
                return runtimeAssetGroups[0];
            }

            // Filter the list by operating system
            var osAssetGroups = runtimeAssetGroups.Where(x => GetOSPlatform().Any(s => x.Runtime.Contains(s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (osAssetGroups.Count == 0)
            {
                Debug.WriteLine(string.Format(NoRuntimeTargetsFoundForOS, String.Join(", ", GetOSPlatform()), assemblyName));
                return null;
            }
            if (osAssetGroups.Count == 1)
            {
                return osAssetGroups[0];
            }

            // Filter the list by os and os architecture
            var osArchitectureAssetGroups = osAssetGroups.Where(x => x.Runtime.Contains($"-{GetProcessArchitecture()}", StringComparison.OrdinalIgnoreCase)).ToList();
            if (osArchitectureAssetGroups.Count == 0)
            {
                Debug.WriteLine(string.Format(NoRuntimeTargetsFoundForOSArchitecture, String.Join(", ", GetOSPlatform()),
                                                                                      GetProcessArchitecture(),
                                                                                      assemblyName));
                return null;
            }
            if (osArchitectureAssetGroups.Count == 1)
            {
                return osArchitectureAssetGroups[0];
            }

            // Finally more than one assembly remaining... told u, a real naive implementation
            throw new EvolveException(string.Format(MultipleRuntimeTargetsFound, assemblyName));
        }

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
                    throw new EvolveException(string.Format(RuntimeLibraryLoadingError, assemblyName, _depsFile), ex);
                }
            }
        }

        /// <summary>
        ///     Returns whether the application is an x64 or x86 process.
        /// </summary>
        /// <returns> x64 or x86 </returns>
        private static string GetProcessArchitecture()
        {
#if NETCORE
            return RuntimeInformation.ProcessArchitecture.ToString();
#else
            return Environment.Is64BitProcess ? "x64" : "x86";
#endif
        }

        /// <summary>
        ///     Determine the operating system on which the application is running.
        /// </summary>
        /// <returns> The list of compatible os. </returns>
        /// <exception cref="PlatformNotSupportedException"> 
        ///     Throws a PlatformNotSupportedException when the the os is neither based on Windows, Linux or OSX. 
        /// </exception>
        private static IEnumerable<string> GetOSPlatform()
        {
#if NETCORE
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new List<string> { "win" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new List<string> { "linux", "unix" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new List<string> { "osx", "unix" };
            }

            throw new PlatformNotSupportedException();
#else
            return new List<string> { "win" };
#endif
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
                throw new EvolveException(string.Format(DependencyContextLoadingError, depsFile), ex);
            }
        }

#if NETCORE

        /// <summary>
        ///     Class responsible for dynamic load of assemblies in .NET Core
        /// </summary>
        private class CustomAssemblyLoader : AssemblyLoadContext
        {
            private readonly CoreReflectionBasedDriver _driverLoader;

            public CustomAssemblyLoader(CoreReflectionBasedDriver driverLoader)
            {
                _driverLoader = driverLoader;

                Resolving += CustomAssemblyLoader_Resolving;
            }

            private Assembly CustomAssemblyLoader_Resolving(AssemblyLoadContext context, AssemblyName assemblyName)
            {
                RuntimeLibrary lib = _driverLoader.GetRuntimeLibrary(assemblyName.Name);
                string assemblyPath = _driverLoader.GetAssemblyPath(lib);
                return context.LoadFromAssemblyPath(assemblyPath);
            }

            protected override Assembly Load(AssemblyName assemblyName) => null;

            /// <summary>
            ///     Load an unmanaged dependency where the path was previously stored in <see cref="CoreReflectionBasedDriver.NativeDependencies"/>
            /// </summary>
            /// <param name="unmanagedDllName"> The name of the dll to load. </param>
            /// <returns></returns>
            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                // hack for the SQLClient driver: remove all native dependencies that do not target the current ProcessArchitecture
                _driverLoader.NativeDependencies.RemoveAll(x => x.Contains(RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "x86" : "x64"));
                if (RuntimeInformation.ProcessArchitecture.ToString().StartsWith("X"))
                {
                    _driverLoader.NativeDependencies.RemoveAll(x => x.Contains("arm", StringComparison.OrdinalIgnoreCase));
                }

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