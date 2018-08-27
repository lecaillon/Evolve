#if NET

using System;
using System.Collections.Generic;
using Evolve.Driver;
using Evolve.Utilities;

namespace Evolve.Connection
{
    /// <summary>
    ///     Base class for obtaining a <see cref="WrappedConnection"/> from a .NET driver.
    /// </summary>
    public class DriverConnectionProvider : IConnectionProvider
    {
        private const string UnknownDriver = "Driver name {0} is unknown. Try one of the following: Microsoft.Sqlite, SQLite, MySQL.Data, MariaDB, Npgsql, SqlClient.";
        private const string DriverCreationError = "Database driver creation error. Verify that your driver assembly is in the same folder that your application.";

        private readonly string _driverName;
        private readonly string _connectionString;
        private WrappedConnection _wrappedConnection;

        /// <summary>
        ///     Map a driver name to its <see cref="IDriver"/> implementation.
        /// </summary>
        private readonly Dictionary<string, Func<IDriver>> _driverMap = new Dictionary<string, Func<IDriver>>
        {
            ["microsoftdatasqlite"] = () => new MicrosoftDataSqliteDriver(),
            ["microsoftsqlite"]     = () => new MicrosoftDataSqliteDriver(),

            ["sqlite"]              = () => new SystemDataSQLiteDriver(),
            ["systemdatasqlite"]    = () => new SystemDataSQLiteDriver(),

            ["mysql"]               = () => new MySqlDataDriver(),
            ["mariadb"]             = () => new MySqlDataDriver(),
            ["mysqldata"]           = () => new MySqlDataDriver(),
            ["mysqlconnector"]      = () => new MySqlConnectorDriver(),

            ["npgsql"]              = () => new NpgsqlDriver(),
            ["postgresql"]          = () => new NpgsqlDriver(),

            ["sqlserver"]           = () => new SqlClientDriver(),
            ["sqlclient"]           = () => new SqlClientDriver(),

            ["cassandra"]           = () => new CassandraDriver(),
        };

        /// <summary>
        ///     Initializes a new instance of a <see cref="DriverConnectionProvider"/> from the given <paramref name="driverName"/>.
        /// </summary>
        /// <param name="driverName"> Name of the driver to load. </param>
        /// <param name="connectionString"> Connection string used to initialised a database connection. </param>
        public DriverConnectionProvider(string driverName, string connectionString)
        {
            _driverName = Check.NotNullOrEmpty(driverName, nameof(driverName));
            _connectionString = Check.NotNullOrEmpty(connectionString, nameof(connectionString));

            Driver = LoadDriver();
        }

        public IDriver Driver { get; }

        /// <summary>
        ///     Returns a wrapped <see cref="System.Data.IDbConnection"/> initiated by the loaded <see cref="Driver"/>.
        /// </summary>
        /// <returns> A connection to the database to evolve. </returns>
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

            _driverMap.TryGetValue(cleanDriverName, out Func<IDriver> driverCreationDelegate);

            if (driverCreationDelegate == null)
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

#endif