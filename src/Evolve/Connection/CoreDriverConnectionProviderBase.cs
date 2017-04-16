#if NETCORE || NET45

using System;
using System.Collections.Generic;
using Evolve.Driver;
using Evolve.Utilities;

namespace Evolve.Connection
{
    /// <summary>
    ///     Base class for obtaining a <see cref="WrappedConnection"/> from a .NET Core driver.
    /// </summary>
    public abstract class CoreDriverConnectionProviderBase : IConnectionProvider
    {
        private const string UnknownDriver = "Driver name {0} is unknown. Try one of the following: Microsoft.Sqlite, SQLite, MySQL.Data, MariaDB, Npgsql, SqlClient.";
        private const string DriverCreationError = "Database driver creation error.";

        private readonly string _driverName;
        private readonly string _connectionString;
        private readonly string _depsFile;
        private readonly string _nugetPackageDir;
        private WrappedConnection _wrappedConnection;

        /// <summary>
        ///     Initializes a new instance of a <see cref="CoreDriverConnectionProviderBase"/> from the given <paramref name="driverName"/>.
        /// </summary>
        /// <param name="driverName"> Name of the driver to load. </param>
        /// <param name="connectionString"> Connection string used to initialised a database connection. </param>
        /// <param name="depsFile"> Dependency file of the project to migrate. </param>
        /// <param name="nugetPackageDir"> Path to the NuGet package folder. </param>
        public CoreDriverConnectionProviderBase(string driverName, string connectionString, string depsFile, string nugetPackageDir)
        {
            _driverName = Check.NotNullOrEmpty(driverName, nameof(driverName));
            _connectionString = Check.NotNullOrEmpty(connectionString, nameof(connectionString));
            _depsFile = Check.NotNullOrEmpty(depsFile, nameof(depsFile));
            _nugetPackageDir = Check.NotNullOrEmpty(nugetPackageDir, nameof(nugetPackageDir));

            Driver = LoadDriver();
        }

        public IDriver Driver { get; }

        /// <summary>
        ///     Returns a map of driver names and their implementations.
        /// </summary>
        protected abstract Dictionary<string, Func<string, string, IDriver>> DriversMap { get; }

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

            DriversMap.TryGetValue(cleanDriverName, out Func<string, string, IDriver> driverCreationDelegate);

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