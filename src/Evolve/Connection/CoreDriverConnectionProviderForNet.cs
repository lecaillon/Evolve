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
        private readonly Dictionary<string, Func<string, string, IDriver>> _driversMap = new Dictionary<string, Func<string, string, IDriver>>
        {
            ["microsoftdatasqlite"] = (depsFile, nugetPackageDir) => new CoreMicrosoftDataSqliteDriverForNet(depsFile, nugetPackageDir),
            ["microsoftsqlite"]     = (depsFile, nugetPackageDir) => new CoreMicrosoftDataSqliteDriverForNet(depsFile, nugetPackageDir),

            ["npgsql"]              = (depsFile, nugetPackageDir) => new CoreNpgsqlDriverForNet(depsFile, nugetPackageDir),
            ["postgresql"]          = (depsFile, nugetPackageDir) => new CoreNpgsqlDriverForNet(depsFile, nugetPackageDir),

            ["mysql"]               = (depsFile, nugetPackageDir) => new CoreMySqlDataDriverForNet(depsFile, nugetPackageDir),
            ["mariadb"]             = (depsFile, nugetPackageDir) => new CoreMySqlDataDriverForNet(depsFile, nugetPackageDir),
            ["mysqldata"]           = (depsFile, nugetPackageDir) => new CoreMySqlDataDriverForNet(depsFile, nugetPackageDir),

            ["sqlserver"]           = (depsFile, nugetPackageDir) => new SqlClientDriver(),
            ["sqlclient"]           = (depsFile, nugetPackageDir) => new SqlClientDriver(),
        };

        /// <summary>
        ///     Initializes a new instance of a <see cref="CoreDriverConnectionProviderForNet"/> from the given <paramref name="driverName"/>.
        /// </summary>
        /// <param name="driverName"> Name of the driver to load. </param>
        /// <param name="connectionString"> Connection string used to initialised a database connection. </param>
        /// <param name="depsFile"> Dependency file of the project to migrate. </param>
        /// <param name="nugetPackageDir"> Path to the NuGet package folder. </param>
        public CoreDriverConnectionProviderForNet(string driverName, string connectionString, string depsFile, string nugetPackageDir) 
            : base(driverName, connectionString, depsFile, nugetPackageDir)
        {
        }

        /// <summary>
        ///     Returns a map of driver names and their implementations.
        /// </summary>
        protected override Dictionary<string, Func<string, string, IDriver>> DriversMap => _driversMap;
    }
}

#endif