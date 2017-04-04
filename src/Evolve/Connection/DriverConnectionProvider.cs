using System;
using System.Collections.Generic;
using Evolve.Driver;
using Evolve.Utilities;

namespace Evolve.Connection
{
    public class DriverConnectionProvider : IConnectionProvider
    {
        private const string UnknownDriver = "Driver name {0} is unknown. Try one of the following: Microsoft.Sqlite, SQLite, MySQL.Data, MariaDB, Npgsql, SqlClient.";

        private readonly string _driverName;
        private readonly string _connectionString;
        private WrappedConnection _wrappedConnection;

#if NETCORE

        private const string DriverCreationError = "Database driver creation error.";

        private readonly Dictionary<string, Func<string, string, IDriver>> _driverMap = new Dictionary<string, Func<string, string, IDriver>>
        {
            ["microsoftdatasqlite"] = (depsFile, nugetPackageDir) => new MicrosoftDataSqliteDriver(depsFile, nugetPackageDir),
            ["microsoftsqlite"]     = (depsFile, nugetPackageDir) => new MicrosoftDataSqliteDriver(depsFile, nugetPackageDir),

            ["sqlite"]              = (depsFile, nugetPackageDir) => new SystemDataSQLiteDriver(depsFile, nugetPackageDir),
            ["systemdatasqlite"]    = (depsFile, nugetPackageDir) => new SystemDataSQLiteDriver(depsFile, nugetPackageDir),

            ["mysql"]               = (depsFile, nugetPackageDir) => new MySqlDataDriver(depsFile, nugetPackageDir),
            ["mariadb"]             = (depsFile, nugetPackageDir) => new MySqlDataDriver(depsFile, nugetPackageDir),
            ["mysqldata"]           = (depsFile, nugetPackageDir) => new MySqlDataDriver(depsFile, nugetPackageDir),

            ["npgsql"]              = (depsFile, nugetPackageDir) => new NpgsqlDriver(depsFile, nugetPackageDir),
            ["postgresql"]          = (depsFile, nugetPackageDir) => new NpgsqlDriver(depsFile, nugetPackageDir),

            ["sqlserver"]           = (depsFile, nugetPackageDir) => new SqlClientDriver(depsFile, nugetPackageDir),
            ["sqlclient"]           = (depsFile, nugetPackageDir) => new SqlClientDriver(depsFile, nugetPackageDir),
        };

#else

        private const string DriverCreationError = "Database driver creation error. Verify that your driver assembly is in the same folder that your application.";

        private readonly Dictionary<string, Func<IDriver>> _driverMap = new Dictionary<string, Func<IDriver>>
        {
            ["microsoftdatasqlite"] = () => new MicrosoftDataSqliteDriver(),
            ["microsoftsqlite"]     = () => new MicrosoftDataSqliteDriver(),

            ["sqlite"]              = () => new SystemDataSQLiteDriver(),
            ["systemdatasqlite"]    = () => new SystemDataSQLiteDriver(),

            ["mysql"]               = () => new MySqlDataDriver(),
            ["mariadb"]             = () => new MySqlDataDriver(),
            ["mysqldata"]           = () => new MySqlDataDriver(),

            ["npgsql"]              = () => new NpgsqlDriver(),
            ["postgresql"]          = () => new NpgsqlDriver(),

            ["sqlserver"]           = () => new SqlClientDriver(),
            ["sqlclient"]           = () => new SqlClientDriver(),
        };

#endif

#if NETCORE

        private readonly string _depsFile;
        private readonly string _nugetPackageDir;

        public DriverConnectionProvider(string driverName, string connectionString, string depsFile, string nugetPackageDir)
        {
            _driverName = Check.NotNullOrEmpty(driverName, nameof(driverName));
            _connectionString = Check.NotNullOrEmpty(connectionString, nameof(connectionString));
            _depsFile = Check.NotNullOrEmpty(depsFile, nameof(depsFile));
            _nugetPackageDir = Check.NotNullOrEmpty(nugetPackageDir, nameof(nugetPackageDir));

            Driver = LoadDriver();
        }

#else

        public DriverConnectionProvider(string driverName, string connectionString)
        {
            _driverName = Check.NotNullOrEmpty(driverName, nameof(driverName));
            _connectionString = Check.NotNullOrEmpty(connectionString, nameof(connectionString));

            Driver = LoadDriver();
        }

#endif

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

#if NETCORE
            _driverMap.TryGetValue(cleanDriverName, out Func<string, string, IDriver> driverCreationDelegate);
#else
            _driverMap.TryGetValue(cleanDriverName, out Func<IDriver> driverCreationDelegate);
#endif

            if (driverCreationDelegate == null)
            {
                throw new EvolveConfigurationException(string.Format(UnknownDriver, _driverName));
            }

            try
            {
#if NETCORE
                return driverCreationDelegate(_depsFile, _nugetPackageDir);
#else
                return driverCreationDelegate();
#endif
            }
            catch (Exception ex)
            {
                throw new EvolveException(DriverCreationError, ex);
            }
        }
    }
}
