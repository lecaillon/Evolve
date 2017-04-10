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
    public abstract class CoreReflectionBasedDriverForNet : CoreReflectionBasedDriver
    {
        private const string WorkingDirectoryCreationError = "Failed to create the driver temp working folder at {0}.";

        public CoreReflectionBasedDriverForNet(string driverAssemblyName, string connectionTypeName, string depsFile, string nugetPackageDir) 
            : base(driverAssemblyName, connectionTypeName, depsFile, nugetPackageDir)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentAppDomain_AssemblyResolve;
        }

        public override IDbConnection CreateConnection(string connectionString)
        {
            Check.NotNullOrEmpty(connectionString, nameof(connectionString));

            var cnn = (IDbConnection)Activator.CreateInstance(DbConnectionType);
            cnn.ConnectionString = connectionString;
            if (NativeDependencies.Count == 0)
            {
                return cnn;
            }

            string originalCurrentDirectory = Directory.GetCurrentDirectory();
            try
            {
                string tempDir = CreateTempDir();
                Directory.SetCurrentDirectory(tempDir);
                foreach (var source in NativeDependencies)
                {
                    string dest = Path.Combine(tempDir, Path.GetFileName(source));
                    if (!File.Exists(dest))
                    {
                        File.Copy(source, dest);
                    }
                }

                cnn.Open();
                cnn.Close();
            }
            catch { }
            finally
            {
                Directory.SetCurrentDirectory(originalCurrentDirectory);
            }

            return cnn;
        }

        protected override Type TypeFromAssembly()
        {
            var lib = base.GetRuntimeLibrary(DriverTypeName.Assembly);

            base.StoreDriverNativeDependencies(lib);

            string driverPath = GetAssemblyPath(lib);
            var driverAssembly = Assembly.LoadFile(driverPath);
            return driverAssembly.GetType(DriverTypeName.Type);
        }

        protected override string GetAssemblyPath(RuntimeLibrary lib)
        {
            string coreAssemblyPath = base.GetAssemblyPath(lib);
            string coreAssemblyName = Path.GetFileName(coreAssemblyPath);
            DirectoryInfo packageFolder = Directory.GetParent(Path.GetDirectoryName(coreAssemblyPath));
            string netAssemblyFolder = packageFolder.GetDirectories("*net4*", SearchOption.TopDirectoryOnly)
                                                    .Where(x => Regex.Match(x.Name, @"net4[5-6](\d)*").Success)
                                                    .Max(x => x.FullName);

            return Path.Combine(netAssemblyFolder, coreAssemblyName);
        }

        private Assembly CurrentAppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyName = args.Name.Split(',')[0].Trim();
            var assemblyVersion = new AssemblyVersion(args.Name.Split(',')[1].Trim().Replace("Version=", string.Empty));
            string packageFolder = Path.Combine(NugetPackageDir, assemblyName);
            string assemblyFolder = Directory.GetDirectories(packageFolder, "*", SearchOption.TopDirectoryOnly)
                                          .Select(x => new DirectoryInfo(x))
                                          .ToLookup(x => new AssemblyVersion(Regex.Match(x.Name, "^[0-9](?:.[0-9]+)*").Value), x => x)
                                          .Where(x => x.Key >= assemblyVersion)
                                          .OrderBy(x => x.Key).First().ToList().First()
                                          .FullName;

            assemblyFolder = Path.Combine(assemblyFolder, "lib");
            string netAssemblyFolder = Directory.GetDirectories(assemblyFolder, "*net4*", SearchOption.TopDirectoryOnly)
                                                .Select(x => new DirectoryInfo(x))
                                                .Where(x => Regex.Match(x.Name, @"net4[5-6](\d)*").Success)
                                                .Max(x => x.FullName);

            return Assembly.LoadFile(Path.Combine(netAssemblyFolder, assemblyName + ".dll"));
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
        /// <exception cref="EvolveException"> Throws an EvolveException when the creation fails. </exception>
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
                throw new EvolveException(string.Format(WorkingDirectoryCreationError, tempDir), ex);
            }
        }

        private class AssemblyVersion : IComparable<AssemblyVersion>, IComparable
        {
            private const string InvalidVersionPatternMatching = "version {0} is invalid. Version must respect this regex: ^[0-9]+(?:.[0-9]+)*$";
            private const string InvalidObjectType = "Object must be of type AssemblyVersion.";

            public AssemblyVersion(string version)
            {
                Version = Check.NotNullOrEmpty(version, nameof(version));

                if (!MatchPattern.IsMatch(Version))
                    throw new EvolveException(string.Format(InvalidVersionPatternMatching, Version));

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