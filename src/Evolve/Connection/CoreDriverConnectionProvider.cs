#if NETSTANDARD

using System;
using System.Collections.Generic;
using Evolve.Driver;
using Evolve.Utilities;

namespace Evolve.Connection
{
    public class CoreDriverConnectionProvider : IConnectionProvider
    {
        private const string UnknownDriver = "Driver name {0} is unknown. Try one of the following: Microsoft.Sqlite, SQLite, MySQL.Data, MariaDB, Npgsql, SqlClient.";
        private const string DriverCreationError = "Database driver creation error.";

        private readonly string _driverName;
        private readonly string _connectionString;
        private readonly string _depsFile;
        private readonly string _nugetPackageDir;
        private WrappedConnection _wrappedConnection;

        private readonly Dictionary<string, Func<string, string, IDriver>> _driverMap = new Dictionary<string, Func<string, string, IDriver>>
        {
            ["microsoftdatasqlite"] = (depsFile, nugetPackageDir) => new MicrosoftDataSqliteDriver(depsFile, nugetPackageDir),
            ["microsoftsqlite"] = (depsFile, nugetPackageDir) => new MicrosoftDataSqliteDriver(depsFile, nugetPackageDir),

            ["sqlite"] = (depsFile, nugetPackageDir) => new SystemDataSQLiteDriver(depsFile, nugetPackageDir),
            ["systemdatasqlite"] = (depsFile, nugetPackageDir) => new SystemDataSQLiteDriver(depsFile, nugetPackageDir),

            ["mysql"] = (depsFile, nugetPackageDir) => new MySqlDataDriver(depsFile, nugetPackageDir),
            ["mariadb"] = (depsFile, nugetPackageDir) => new MySqlDataDriver(depsFile, nugetPackageDir),
            ["mysqldata"] = (depsFile, nugetPackageDir) => new MySqlDataDriver(depsFile, nugetPackageDir),

            ["npgsql"] = (depsFile, nugetPackageDir) => new NpgsqlDriver(depsFile, nugetPackageDir),
            ["postgresql"] = (depsFile, nugetPackageDir) => new NpgsqlDriver(depsFile, nugetPackageDir),

            ["sqlserver"] = (depsFile, nugetPackageDir) => new SqlClientDriver(depsFile, nugetPackageDir),
            ["sqlclient"] = (depsFile, nugetPackageDir) => new SqlClientDriver(depsFile, nugetPackageDir),
        };

        public CoreDriverConnectionProvider(string driverName, string connectionString, string depsFile, string nugetPackageDir)
        {
            _driverName = Check.NotNullOrEmpty(driverName, nameof(driverName));
            _connectionString = Check.NotNullOrEmpty(connectionString, nameof(connectionString));
            _depsFile = Check.NotNullOrEmpty(depsFile, nameof(depsFile));
            _nugetPackageDir = Check.NotNullOrEmpty(nugetPackageDir, nameof(nugetPackageDir));

            Driver = LoadDriver();
        }

        public IDriver Driver { get; }

        public WrappedConnection GetConnection()
        {
            if (_wrappedConnection == null)
            {
                var connection = Driver.CreateConnection(_connectionString);
                _wrappedConnection = new WrappedConnection(connection);
            }

            return _wrappedConnection;
        }

        private IDriver LoadDriver()
        {
            string cleanDriverName = _driverName.ToLowerInvariant()
                                                .Replace(" ", "")
                                                .Replace(".", "");

            _driverMap.TryGetValue(cleanDriverName, out Func<string, string, IDriver> driverCreationDelegate);

            if (driverCreationDelegate == null)
            {
                throw new EvolveConfigurationException(string.Format(UnknownDriver, _driverName));
            }

            try
            {
                return driverCreationDelegate(_depsFile, _nugetPackageDir);
            }
            catch (Exception ex)
            {
                throw new EvolveException(DriverCreationError, ex);
            }
        }
    }
}

#endif