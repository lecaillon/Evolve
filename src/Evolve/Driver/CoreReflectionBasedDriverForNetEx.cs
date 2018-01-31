#if NET45

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Evolve.Utilities;
using Microsoft.Extensions.DependencyModel;

namespace Evolve.Driver
{
    /// <summary>
    ///     <para>
    ///         Base class for .NET Standard/Core drivers loaded by reflection by MSBuild.
    ///     </para>
    ///     <para>
    ///         Because MSBuild does not support the .NET Core class <see cref="AssemblyLoadContext"/> (see https://github.com/Microsoft/msbuild/issues/1940)
    ///         we can not use the <see cref="CoreReflectionBasedDriver"/> to load a driver of a .NET Standard/Core project.
    ///         We have to implement a .NET class for that.
    ///         
    ///         The idea is to locate the .NET Core driver assembly with the .deps.json file and then find its equivalent .NET version from it.
    ///     </para>
    /// </summary>
    public abstract class CoreReflectionBasedDriverForNetEx : CoreReflectionBasedDriverEx
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="CoreReflectionBasedDriverForNet" /> with
        ///     the connection type name loaded from the specified assembly.
        /// </summary>
        /// <param name="driverAssemblyName"> Assembly to load the driver Type from. </param>
        /// <param name="connectionTypeName"> Name of the driver Type. </param>
        /// <param name="depsFile"> Dependency file of the project to migrate. </param>
        /// <param name="nugetPackageDir"> Path to the NuGet package folder. </param>
        public CoreReflectionBasedDriverForNetEx(string driverAssemblyName, string connectionTypeName, string depsFile, string nugetPackageDir) 
            : base(driverAssemblyName, connectionTypeName, depsFile, nugetPackageDir)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentAppDomain_AssemblyResolve;
        }

        /// <summary>
        ///     <para>
        ///         Creates an <see cref="IDbConnection"/> object for the specific driver.
        ///     </para>
        ///     <para>
        ///         If the driver has native dependencies, we create a temp folder where they are copied and loaded from.
        ///         To do that, we change the application current directory to the temp folder and open a connection to the database.
        ///     </para>
        /// </summary>
        /// <param name="connectionString"> The connection string used to initialize the IDbConnection. </param>
        /// <returns> An initialized database connection. </returns>
        /// <exception cref="EvolveCoreDriverException"></exception>
        public override IDbConnection CreateConnection(string connectionString)
        {
            Check.NotNullOrEmpty(connectionString, nameof(connectionString));

            IDbConnection cnn = null;
            Type cnxType = DbConnectionType;

            if (NativeDependencies.Count == 0)
            {
                try
                {
                    cnn = (IDbConnection)Activator.CreateInstance(cnxType);
                    cnn.ConnectionString = connectionString;
                    return cnn;
                }
                catch (Exception ex)
                {
                    throw new EvolveCoreDriverException($"Error creating an instance of the type {cnxType}", DumpDetails(), ex);
                }
            }

            string originalCurrentDirectory = Directory.GetCurrentDirectory();
            string tempDir = CreateTempDir();
            try
            {
                Directory.SetCurrentDirectory(tempDir);
                foreach (var source in NativeDependencies)
                {
                    string dest = Path.Combine(tempDir, Path.GetFileName(source));
                    if (!File.Exists(dest))
                    {
                        File.Copy(source, dest);
                    }
                }

                try
                {
                    cnn = (IDbConnection)Activator.CreateInstance(cnxType);
                }
                catch (Exception ex)
                {
                    throw new EvolveCoreDriverException($"Error creating an instance of the type {cnxType}", DumpDetails(), ex);
                }

                try
                {
                    cnn.ConnectionString = connectionString;
                    cnn.Open(); // force the load of the native dependencies
                    cnn.Close();
                }
                catch (Exception ex)
                {
                    throw new EvolveCoreDriverException($"Error openning a connection to the database with the previously created {cnxType.Name}", DumpDetails(), ex);
                }

                return cnn;
            }
            finally
            {
                Directory.SetCurrentDirectory(originalCurrentDirectory); // restore applicaiton current directory
            }
        }

        /// <summary>
        ///     <para>
        ///         Find and load the driver <see cref="Type"/> from a .deps file definition.
        ///     </para>
        ///     <para>
        ///         For .NET drivers, we only load the main assembly,
        ///         and not all the managed ones found in <see cref="CoreReflectionBasedDriverEx.ManagedDependencies"/>.
        ///     </para>
        /// </summary>
        /// <returns> The driver type. </returns>
        /// <exception cref="EvolveCoreDriverException"></exception>
        protected override Type TypeFromAssembly()
        {
            ManagedDependencies = new List<string>();
            NativeDependencies = new List<string>();

            RuntimeLibrary rootLib = GetRuntimeLibrary(DriverTypeName.Assembly);
            FindDependencies(rootLib);

            string driverPath = ManagedDependencies.FirstOrDefault(x => x.Contains(DriverTypeName.Assembly));
            if (string.IsNullOrEmpty(driverPath))
            {
                throw new EvolveCoreDriverException($"Assembly {DriverTypeName.Assembly} not found.", DumpDetails());
            }

            try
            {
                var driverAssembly = Assembly.LoadFile(GetNetVersion(driverPath));
                return driverAssembly.GetType(DriverTypeName.Type);
            }
            catch (Exception ex)
            {
                throw new EvolveCoreDriverException($"Error loading driver assembly {DriverTypeName.Type} from {driverPath}", DumpDetails(), ex);
            }
        }

        /// <summary>
        ///     <para>
        ///         Returns if exists, the path of the .NET version of the driver assembly,
        ///         otherwise returns the path of the .NET Core version.
        ///     </para>
        ///     <para>
        ///         From the .NET Standard/Core driver assembly, navigate to the parent directory 
        ///         and search the best compatible .NET version. <see cref="GetClosestCompatibleNetFolder"/>
        ///     </para>
        /// </summary>
        /// <param name="driverPath"> The netcore driver runtime library path. </param>
        /// <returns> The path to the .NET or .NET Core version of the driver assembly. </returns>
        private string GetNetVersion(string driverPath)
        {
            string driverFileName = Path.GetFileName(driverPath);
            string parentFolder = new DirectoryInfo(Path.GetDirectoryName(driverPath)).Parent.FullName;
            string netAssemblyFolder = GetClosestCompatibleNetFolder(parentFolder);

            return netAssemblyFolder == null 
                ? driverPath
                : Path.Combine(netAssemblyFolder, driverFileName);
        }

        /// <summary>
        ///     <para>
        ///         Occurs when loading the driver requires a dependency the current AppDomain does not know.
        ///     </para>
        ///     <para>
        ///         We try to find the required assembly from the <see cref="CoreReflectionBasedDriverEx.ManagedDependencies"/>.
        ///         
        ///         If found, we search for the .NET equivalent and returns it, or returns the .NET Core version if it fails.
        ///         
        ///         If not found, given the path of the NuGet package cache and the name of the dependency, 
        ///         inferred the root folder path of the package assembly to resolve.
        ///         From there, given the version of the dependency, find the folder which matches best: 
        ///         take the equal or higher closest version available.
        ///         Finally, find the the best compatible .NET version with <see cref="GetClosestCompatibleNetFolder"/> or throws 
        ///         an EvolveCoreDriverException if it fails.
        ///     </para>
        /// </summary>
        /// <returns> The resolved assembly. </returns>
        /// <exception cref="EvolveCoreDriverException"></exception>
        private Assembly CurrentAppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyName = "";
            AssemblyVersion assemblyVersion = null;

            try
            {
                assemblyName = args.Name.Split(',')[0].Trim();
                assemblyVersion = new AssemblyVersion(args.Name.Split(',')[1].Trim().Replace("Version=", string.Empty));
            }
            catch
            {
                throw new EvolveCoreDriverException($"Assembly {args.Name} has no versioning information in its name.", DumpDetails());
            }

            string packageFolder = "";
            string versionedPackageFolder = "";
            string netCorePath = ManagedDependencies.FirstOrDefault(x => x.Contains(assemblyName + ".dll"));
            if (netCorePath == null)
            {
                // The requested dll is not in the ManagedDependencies list.
                // Try to find the .NET version of it in the Nuget cache folder.
                packageFolder = Path.Combine(NugetPackageDir, assemblyName);
                if (!Directory.Exists(packageFolder))
                {
                    throw new EvolveCoreDriverException($"Can't resolve {args.Name}.\r\n Folder {packageFolder} not found.", DumpDetails());
                }

                versionedPackageFolder = Directory.GetDirectories(packageFolder, "*", SearchOption.TopDirectoryOnly)
                                                  .Select(x => new DirectoryInfo(x))
                                                  .Where(x => Regex.IsMatch(x.Name, "^[0-9]+(?:.[0-9]+)*")) // guard
                                                  .ToLookup(x => new AssemblyVersion(Regex.Match(x.Name, "^[0-9]+(?:.[0-9]+)*").Value), x => x) // extract the version of the available packages
                                                  .Where(x => x.Key >= assemblyVersion) // keep only versions equal or higher than the requested
                                                  .OrderBy(x => x.Key).First().ToList().First() // take the closest
                                                  .FullName;

                if (string.IsNullOrEmpty(versionedPackageFolder))
                {
                    throw new EvolveCoreDriverException($"Can't resolve {args.Name}. No folder named with a version found in {versionedPackageFolder}", DumpDetails());
                }

                // Try to find the net4* folder in the /lib folder
                string netAssemblyFolder = GetClosestCompatibleNetFolder(Path.Combine(versionedPackageFolder, "lib"));
                if (netAssemblyFolder == null)
                {
                    throw new EvolveCoreDriverException($"Can't resolve {args.Name}. No net4* compatible folder found in {versionedPackageFolder}", DumpDetails());
                }

                return Assembly.LoadFile(Path.Combine(netAssemblyFolder, assemblyName + ".dll"));
            }
            else
            {
                // Try to find the equivalent .NET dll of the .NET Core ManagedDependencies
                packageFolder = new DirectoryInfo(Path.GetDirectoryName(netCorePath)).Parent.FullName;
                string netAssemblyFolder = GetClosestCompatibleNetFolder(packageFolder);

                return netAssemblyFolder == null
                    ? Assembly.LoadFile(netCorePath) // Can't find the .NET assembly, so load the .NET Core one
                    : Assembly.LoadFile(Path.Combine(netAssemblyFolder, assemblyName + ".dll"));
            }
        }

        /// <summary>
        ///     Returns the compatible .NET folder from the given <paramref name="rootDir"/>.
        /// </summary>
        /// <param name="rootDir"> The root search directory. </param>
        /// <returns> The full path of the compatible folder, or null if not found. </returns>
        private string GetClosestCompatibleNetFolder(string rootDir)
        {
            List<DirectoryInfo> candidates = Directory.GetDirectories(rootDir).Select(x => new DirectoryInfo(x)).ToList();
            string compatibleFolder = candidates.Where(x => Regex.IsMatch(x.Name, @"^net4[5-7](\d)*")).Max(x => x.FullName);
            if (!string.IsNullOrEmpty(compatibleFolder))
            {
                return compatibleFolder;
            }

            compatibleFolder = candidates.Where(x => Regex.IsMatch(x.Name, @"portable-net4[5-7](\d)*")).Max(x => x.FullName);
            if (!string.IsNullOrEmpty(compatibleFolder))
            {
                return compatibleFolder;
            }

            compatibleFolder = candidates.Where(x => Regex.IsMatch(x.Name, @"^netstandard(\d)*")).Min(x => x.FullName);
            if (!string.IsNullOrEmpty(compatibleFolder))
            {
                return compatibleFolder;
            }

            return null;
        }

        /// <summary>
        ///     <para>
        ///         Creates a folder used as a temp directory where the 
        ///         driver native dependency assemblies are copied to and loaded from.
        ///     </para>
        ///     <para>
        ///         This folder is created in the user temp directory to avoid permission issues.
        ///     </para>
        /// </summary>
        /// <returns> Path to the temp directory. </returns>
        /// <exception cref="EvolveCoreDriverException"> Throws an EvolveCoreDriverException when the creation fails. </exception>
        private string CreateTempDir()
        {
            string tempDir = "";
            try
            {
                tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempDir);
                return tempDir;
            }
            catch (Exception ex)
            {
                throw new EvolveCoreDriverException($"Failed to create the driver temp working folder at {tempDir}.", ex);
            }
        }

        /// <summary>
        ///     Convenient class for assembly version comparaison. Inspired by <see cref="Evolve.Migration.MigrationVersion"/> 
        /// </summary>
        private class AssemblyVersion : IComparable<AssemblyVersion>, IComparable
        {
            private const string InvalidVersionPatternMatching = "version {0} is invalid. Version must respect this regex: ^[0-9]+(?:.[0-9]+)*$";
            private const string InvalidObjectType = "Object must be of type AssemblyVersion.";

            /// <summary>
            ///     Initializes a new instance of the <see cref="AssemblyVersion"/> class.
            /// </summary>
            /// <exception cref="EvolveCoreDriverException"></exception>
            public AssemblyVersion(string version)
            {
                Version = Check.NotNullOrEmpty(version, nameof(version));

                if (!MatchPattern.IsMatch(Version))
                {
                    throw new EvolveCoreDriverException(string.Format(InvalidVersionPatternMatching, Version));
                }

                VersionParts = Version.Split('.').Select(long.Parse).ToList();
            }

            public static Regex MatchPattern => new Regex("^[0-9]+(?:.[0-9]+)*$");
            public string Version { get; }
            public List<long> VersionParts { get; set; }

            #region IComparable

            public int CompareTo(AssemblyVersion other)
            {
                if (other == null) return 1;

                using (IEnumerator<long> e1 = VersionParts.GetEnumerator())
                using (IEnumerator<long> e2 = other.VersionParts.GetEnumerator())
                {
                    while (e1.MoveNext())
                    {
                        if (!e2.MoveNext())
                            return 1;

                        if (e1.Current.CompareTo(e2.Current) == 0)
                            continue;

                        return e1.Current.CompareTo(e2.Current);
                    }
                    return e2.MoveNext() ? -1 : 0;
                }
            }

            public int CompareTo(object obj)
            {
                if (obj != null && !(obj is AssemblyVersion))
                    throw new ArgumentException(InvalidObjectType);

                return CompareTo(obj as AssemblyVersion);
            }

            #endregion

            #region Operators

            public override bool Equals(object obj) => (CompareTo(obj as AssemblyVersion) == 0);

            public static bool operator ==(AssemblyVersion operand1, AssemblyVersion operand2)
            {
                if (ReferenceEquals(operand1, null))
                {
                    return ReferenceEquals(operand2, null);
                }

                return operand1.Equals(operand2);
            }

            public static bool operator !=(AssemblyVersion operand1, AssemblyVersion operand2) => !(operand1 == operand2);

            public static bool operator >(AssemblyVersion operand1, AssemblyVersion operand2) => operand1.CompareTo(operand2) == 1;

            public static bool operator <(AssemblyVersion operand1, AssemblyVersion operand2) => operand1.CompareTo(operand2) == -1;

            public static bool operator >=(AssemblyVersion operand1, AssemblyVersion operand2) => operand1.CompareTo(operand2) >= 0;

            public static bool operator <=(AssemblyVersion operand1, AssemblyVersion operand2) => operand1.CompareTo(operand2) <= 0;

            #endregion

            public override int GetHashCode() => Version.GetHashCode();

            public override string ToString() => Version;
        }
    }
}

#endif
