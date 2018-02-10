#if NET45

using System;
using System.Collections.Generic;
using Evolve.Driver;

namespace Evolve.Connection
{
    /// <summary>
    ///     A startegy for obtaining a <see cref="WrappedConnection"/> from a .NET Core driver in a MSBuild .NET context.
    /// </summary>
    public class CoreDriverConnectionProviderForNet : CoreDriverConnectionProviderBase
    {
        /// <summary>
        ///     Map a driver name to its <see cref="IDriver"/> implementation.
        /// </summary>
        private readonly Dictionary<string, Func<string, string, string, IDriver>> _driversMap = new Dictionary<string, Func<string, string, string, IDriver>>
        {
            ["microsoftdatasqlite"] = (depsFile, nugetPackageDir, msBuildExtensionsPath) => new CoreMicrosoftDataSqliteDriverForNet(depsFile, nugetPackageDir, msBuildExtensionsPath),
            ["microsoftsqlite"]     = (depsFile, nugetPackageDir, msBuildExtensionsPath) => new CoreMicrosoftDataSqliteDriverForNet(depsFile, nugetPackageDir, msBuildExtensionsPath),

            ["npgsql"]              = (depsFile, nugetPackageDir, msBuildExtensionsPath) => new CoreNpgsqlDriverForNet(depsFile, nugetPackageDir, msBuildExtensionsPath),
            ["postgresql"]          = (depsFile, nugetPackageDir, msBuildExtensionsPath) => new CoreNpgsqlDriverForNet(depsFile, nugetPackageDir, msBuildExtensionsPath),

            ["mysql"]               = (depsFile, nugetPackageDir, msBuildExtensionsPath) => new CoreMySqlDataDriverForNet(depsFile, nugetPackageDir, msBuildExtensionsPath),
            ["mariadb"]             = (depsFile, nugetPackageDir, msBuildExtensionsPath) => new CoreMySqlDataDriverForNet(depsFile, nugetPackageDir, msBuildExtensionsPath),
            ["mysqldata"]           = (depsFile, nugetPackageDir, msBuildExtensionsPath) => new CoreMySqlDataDriverForNet(depsFile, nugetPackageDir, msBuildExtensionsPath),

            ["sqlserver"]           = (depsFile, nugetPackageDir, _) => new SqlClientDriver(),
            ["sqlclient"]           = (depsFile, nugetPackageDir, _) => new SqlClientDriver(),
        };

        /// <summary>
        ///     Initializes a new instance of a <see cref="CoreDriverConnectionProviderForNet"/> from the given <paramref name="driverName"/>.
        /// </summary>
        /// <param name="driverName"> Name of the driver to load. </param>
        /// <param name="connectionString"> Connection string used to initialised a database connection. </param>
        /// <param name="depsFile"> Dependency file of the project to migrate. </param>
        /// <param name="nugetPackageDir"> Path to the NuGet package folder. </param>
        /// <param name="msBuildExtensionsPath"> Path to the MSBuild extension folder, used by Evolve when loading .NET Core 2 driver via .NET MSBuild. </param>
        public CoreDriverConnectionProviderForNet(string driverName, string connectionString, string depsFile, string nugetPackageDir, string msBuildExtensionsPath = null) 
            : base(driverName, connectionString, depsFile, nugetPackageDir, msBuildExtensionsPath)
        {
        }

        /// <summary>
        ///     Returns a map of driver names and their implementations.
        /// </summary>
        protected override Dictionary<string, Func<string, string, string, IDriver>> DriversMap => _driversMap;
    }
}

#endif