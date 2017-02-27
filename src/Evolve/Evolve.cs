using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Evolve.Configuration;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Migration;
using Evolve.Utilities;

namespace Evolve
{
    public class Evolve : IEvolveConfiguration, IMigrator
    {
        private const string IncorrectChecksum = "Checksum validation failed for script: {0}.";
        private const string MigrationMetadataNotFound = "Script {0} not found in the metadata table of applied migrations.";

        public Evolve()
        {
            // Set default configuration settings
            Schemas = new List<string>();
            Encoding = Encoding.UTF8;
            Locations = new List<string> { "Sql_Scripts" };
            MetadaTableName = "changelog";
            PlaceholderPrefix = "${";
            PlaceholderSuffix = "}";
            SqlMigrationPrefix = "V";
            SqlMigrationSeparator = "__";
            SqlMigrationSuffix = ".sql";
        }

        #region IEvolveConfiguration

        public string ConnectionString { get; set; }
        public IEnumerable<string> Schemas { get; set; }
        public string Driver { get; set; }
        public Encoding Encoding { get; set; }
        public IEnumerable<string> Locations { get; set; }
        public string MetadaTableName { get; set; }

        private string _metadaTableSchema;
        public string MetadataTableSchema
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_metadaTableSchema) ? _metadaTableSchema
                                                                      : Schemas?.FirstOrDefault();
            }
            set { _metadaTableSchema = value; }
        }

        public string PlaceholderPrefix { get; set; }
        public string PlaceholderSuffix { get; set; }
        public string SqlMigrationPrefix { get; set; }
        public string SqlMigrationSeparator { get; set; }
        public string SqlMigrationSuffix { get; set; }
        public MigrationVersion TargetVersion { get; set; }

        #endregion

        public IEnumerable<MigrationScript> ValidatedScripts { get; private set; }

        #region IMigrator

        public string GenerateScript(string fromMigration = null, string toMigration = null)
        {
            throw new NotImplementedException();
        }

        public void Migrate(string targetVersion = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void Validate(DatabaseHelper database, IMigrationLoader loader)
        {
            Check.NotNull(database, nameof(database));
            Check.NotNull(loader, nameof(loader));

            var metadataTable = database.GetMetadataTable(MetadataTableSchema, MetadaTableName);                                // Get the metadata table
            if(!metadataTable.IsExists())
            {
                return; // Nothing to validate
            }

            var appliedMigrations = metadataTable.GetAllMigrationMetadata();                                                    // Load all applied migrations metadata
            if (appliedMigrations.Count() == 0)
            {
                return; // Nothing to validate
            }

            ValidatedScripts = loader.GetMigrations(Locations, SqlMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix);  // Load scripts found at Locations
            var lastAppliedVersion = appliedMigrations.Last().Version;                                                          // Get the last applied migration version
            var startVersion = metadataTable.FindStartVersion();                                                                // Load start version from metadata
            var scripts = ValidatedScripts.SkipWhile(x => x.Version < startVersion)
                                          .TakeWhile(x => x.Version <= lastAppliedVersion);                                     // Keep scripts between first and last applied migration

            foreach (var script in scripts)
            {
                var appliedMigration = appliedMigrations.SingleOrDefault(x => x.Version == script.Version);                     // Search script in the applied migrations
                if(appliedMigration == null)
                {
                    throw new EvolveException(string.Format(MigrationMetadataNotFound, script.Name));
                }

                if(script.CalculateChecksum() != appliedMigration.Checksum)                                                     // Script found, verify checksum
                {
                    throw new EvolveException(string.Format(IncorrectChecksum, script.Name));
                }
            }
        }

        private void Initialize(string evolveConfigurationPath, IDbConnection cnn = null)
        {
            Check.FileExists(evolveConfigurationPath, nameof(evolveConfigurationPath));

            Configure(evolveConfigurationPath);                                             // Load configuration
            var connectionProvider = GetConnectionProvider(cnn);                            // Get a database connection provider
            var connection = connectionProvider.GetConnection();                            // Get a connection to the database
            connection.Validate();                                                          // Validate the reliabilty of the initiated connection
            var dbmsType = connection.GetDatabaseServerType();                              // Get the DBMS type
            var database = DatabaseHelperFactory.GetDatabaseHelper(dbmsType, connection);   // Get the DatabaseHelper
            if(Schemas == null || Schemas.Count() == 0)
            {
                Schemas = new List<string> { database.GetCurrentSchemaName() };             // If no schema, get the one associated to the datasource connection
            }
        }

        private void Configure(string evolveConfigurationPath)
        {
            Check.FileExists(evolveConfigurationPath, nameof(evolveConfigurationPath));

            IConfigurationProvider configurationProvider = ConfigurationFactoryProvider.GetProvider(evolveConfigurationPath);
            configurationProvider.Configure(evolveConfigurationPath, this);
        }

        private void ManageSchemas()
        {
            throw new NotImplementedException();
        }

        private IConnectionProvider GetConnectionProvider(IDbConnection connection = null)
        {
            return connection != null ? new ConnectionProvider(connection) as IConnectionProvider
                                      : new DriverConnectionProvider(Driver, ConnectionString);
        }
    }
}
