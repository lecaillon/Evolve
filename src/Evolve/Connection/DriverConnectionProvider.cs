using System;
using System.Collections.Generic;
using Evolve.Driver;
using Evolve.Utilities;

namespace Evolve.Connection
{
    public class DriverConnectionProvider : IConnectionProvider
    {
        private const string UnknownDriver = "Driver name {0} is unknown. Try one of the following: Microsoft.Sqlite, SQLite, MySQL.Data, MariaDB, Npgsql, SqlClient.";

#if NETSTANDARD
        private const string DriverCreationError = "Database driver creation error.";
#else
        private const string DriverCreationError = "Database driver creation error. Verify that your driver assembly is in the same folder that your application.";
#endif

        private readonly Dictionary<string, Func<IDriver>> _driverMap = new Dictionary<string, Func<IDriver>>
        {
            ["microsoftdatasqlite"] = () => new MicrosoftDataSqliteDriver(),
            ["microsoftsqlite"] = () => new MicrosoftDataSqliteDriver(),

            ["sqlite"] = () => new SystemDataSQLiteDriver(),
            ["systemdatasqlite"] = () => new SystemDataSQLiteDriver(),

            ["mysql"] = () => new MySqlDataDriver(),
            ["mariadb"] = () => new MySqlDataDriver(),
            ["mysqldata"] = () => new MySqlDataDriver(),

            ["npgsql"] = () => new NpgsqlDriver(),
            ["postgresql"] = () => new NpgsqlDriver(),

            ["sqlserver"] = () => new SqlClientDriver(),
            ["sqlclient"] = () => new SqlClientDriver(),
        };

        private readonly string _driverName;
        private readonly string _connectionString;
        private IWrappedConnection _wrappedConnection;

        public DriverConnectionProvider(string driverName, string connectionString)
        {
            _driverName = Check.NotNullOrEmpty(driverName, nameof(driverName));
            _connectionString = Check.NotNullOrEmpty(connectionString, nameof(connectionString));

            Driver = LoadDriver();
        }

        public IDriver Driver { get; }

        public IWrappedConnection GetConnection()
        {
            if (_wrappedConnection == null)
            {
                var connection = Driver.CreateConnection();
                connection.ConnectionString = _connectionString;

                _wrappedConnection = new WrappedConnection(connection);
            }

            return _wrappedConnection;
        }

        private IDriver LoadDriver()
        {
            string cleanDriverName = _driverName.ToLowerInvariant()
                                                .Replace(" ", "")
                                                .Replace(".", "");

            _driverMap.TryGetValue(cleanDriverName, out Func<IDriver> driverCreationDelegate);

            if(driverCreationDelegate == null)
            {
                throw new EvolveConfigurationException(string.Format(UnknownDriver, _driverName));
            }

            try
            {
                return driverCreationDelegate();
            }
            catch (Exception ex)
            {
                throw new EvolveException(DriverCreationError, ex);
            }
        }
    }
}
