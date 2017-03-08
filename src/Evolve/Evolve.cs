using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Evolve.Configuration;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Metadata;
using Evolve.Migration;
using Evolve.Utilities;

namespace Evolve
{
    public class Evolve : IEvolveConfiguration, IMigrator
    {
        #region Fields

        private const string IncorrectChecksum = "Checksum validation failed for script: {0}.";
        private const string MigrationMetadataNotFound = "Script {0} not found in the metadata table of applied migrations.";
        private const string NewSchemaCreated = "Create new schema: {0}.";
        private const string EmptySchemaFound = "Empty schema found: {0}.";

        private string _configurationPath;
        private IDbConnection _userDbConnection;
        private IMigrationLoader _loader = new FileMigrationLoader();

        #endregion

        public Evolve(string evolveConfigurationPath, IDbConnection dbConnection = null)
        {
            _configurationPath = Check.FileExists(evolveConfigurationPath, nameof(evolveConfigurationPath));
            _userDbConnection = dbConnection;

            // Set default configuration settings
            Schemas = new List<string>();
            Encoding = Encoding.UTF8;
            Locations = new List<string> { "Sql_Scripts" };
            MetadaTableName = "changelog";
            PlaceholderPrefix = "${";
            PlaceholderSuffix = "}";
            Placeholders = new Dictionary<string, string>();
            SqlMigrationPrefix = "V";
            SqlMigrationSeparator = "__";
            SqlMigrationSuffix = ".sql";

            // Configure Evolve
            var configurationProvider = ConfigurationFactoryProvider.GetProvider(evolveConfigurationPath);
            configurationProvider.Configure(evolveConfigurationPath, this);
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
                return !_metadaTableSchema.IsNullOrWhiteSpace() ? _metadaTableSchema
                                                                : Schemas?.FirstOrDefault();
            }
            set { _metadaTableSchema = value; }
        }

        public string PlaceholderPrefix { get; set; }
        public string PlaceholderSuffix { get; set; }
        public Dictionary<string, string> Placeholders { get; set; }
        public string SqlMigrationPrefix { get; set; }
        public string SqlMigrationSeparator { get; set; }
        public string SqlMigrationSuffix { get; set; }
        public MigrationVersion TargetVersion { get; set; }

        #endregion

        #region IMigrator

        public string GenerateScript(string fromMigration = null, string toMigration = null)
        {
            throw new NotImplementedException();
        }

        public void Migrate(string targetVersion = null)
        {
            Check.NullableButNotEmpty(targetVersion, nameof(targetVersion));

            var db = Initialize();
            Validate(db);
            ManageSchemas(db);

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadaTableName);
            var lastAppliedVersion = metadata.GetAllMigrationMetadata().LastOrDefault()?.Version ?? new MigrationVersion("0");
            var targetMigrationVersion = new MigrationVersion(targetVersion ?? long.MaxValue.ToString());
            var scripts = _loader.GetMigrations(Locations, SqlMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix)
                                 .SkipWhile(x => x.Version <= lastAppliedVersion)
                                 .TakeWhile(x => x.Version <= targetMigrationVersion);

            foreach (var script in scripts)
            {
                try
                {
                    db.WrappedConnection.BeginTransaction();
                    db.WrappedConnection.ExecuteNonQuery(script.LoadSQL(Placeholders, Encoding));
                    metadata.SaveMigration(script, true);
                    db.WrappedConnection.Commit();
                }
                catch
                {
                    db.WrappedConnection.Rollback();
                    metadata.SaveMigration(script, false);
                    throw;
                }
            }
        }

        public void Validate()
        {
            var db = Initialize();
            Validate(db);
        }

        public void Erase()
        {
            var db = Initialize();

            var schemaToDrop = new List<string>();
            var schemaToErase = new List<string>();
            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadaTableName);

            foreach (var schemaName in FindSchemas())
            {
                if(metadata.CanDropSchema(schemaName))
                {
                    schemaToDrop.Add(schemaName);
                }
                else if (metadata.CanEraseSchema(schemaName))
                {
                    schemaToErase.Add(schemaName);
                }
            }

            db.WrappedConnection.BeginTransaction();
            schemaToDrop.ForEach(x => db.GetSchema(x).Drop());
            schemaToErase.ForEach(x => db.GetSchema(x).Erase());
            db.WrappedConnection.Commit();
        }

        #endregion

        private DatabaseHelper Initialize()
        {
            var connectionProvider = GetConnectionProvider(_userDbConnection);              // Get a database connection provider
            var evolveConnection = connectionProvider.GetConnection();                      // Get a connection to the database
            evolveConnection.Validate();                                                    // Validate the reliabilty of the initiated connection
            var dbmsType = evolveConnection.GetDatabaseServerType();                        // Get the DBMS type
            var db = DatabaseHelperFactory.GetDatabaseHelper(dbmsType, evolveConnection);   // Get the DatabaseHelper
            if(Schemas == null || Schemas.Count() == 0)
            {
                Schemas = new List<string> { db.GetCurrentSchemaName() };                   // If no schema, get the one associated to the datasource connection
            }

            return db;
        }

        private void Validate(DatabaseHelper db)
        {
            Check.NotNull(db, nameof(db));

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadaTableName);                                       // Get the metadata table
            if (!metadata.IsExists())                                                                                       
            {                                                                                                               
                return; // Nothing to validate                                                                              
            }                                                                                                               
                                                                                                                            
            var appliedMigrations = metadata.GetAllMigrationMetadata();                                                     // Load all applied migrations metadata
            if (appliedMigrations.Count() == 0)                                                                             
            {                                                                                                               
                return; // Nothing to validate                                                                              
            }                                                                                                               
                                                                                                                            
            var lastAppliedVersion = appliedMigrations.Last().Version;                                                      // Get the last applied migration version
            var startVersion = metadata.FindStartVersion();                                                                 // Load start version from metadata
            var scripts = _loader.GetMigrations(Locations, SqlMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix)
                                 .SkipWhile(x => x.Version < startVersion)                                         
                                 .TakeWhile(x => x.Version <= lastAppliedVersion);                                          // Keep scripts between first and last applied migration
                                                                                                                            
            foreach (var script in scripts)                                                                                 
            {                                                                                                               
                var appliedMigration = appliedMigrations.SingleOrDefault(x => x.Version == script.Version);                 // Search script in the applied migrations
                if (appliedMigration == null)                                                                               
                {                                                                                                           
                    throw new EvolveException(string.Format(MigrationMetadataNotFound, script.Name));                       
                }                                                                                                           
                                                                                                                            
                if (script.CalculateChecksum() != appliedMigration.Checksum)                                                // Script found, verify checksum
                {
                    throw new EvolveException(string.Format(IncorrectChecksum, script.Name));
                }
            }
        }

        private void ManageSchemas(DatabaseHelper db)
        {
            Check.NotNull(db, nameof(db));

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadaTableName);

            foreach (var schemaName in FindSchemas())
            {
                var schema = db.GetSchema(schemaName);

                if(!schema.IsExists())
                {
                    // Create new schema
                    db.WrappedConnection.BeginTransaction();
                    schema.Create();
                    metadata.Save(MetadataType.NewSchema, "0", string.Format(NewSchemaCreated, schemaName), schemaName);
                    db.WrappedConnection.Commit();
                }
                else if(schema.IsEmpty())
                {
                    // Mark schema as empty in the metadata table
                    metadata.Save(MetadataType.EmptySchema, "0", string.Format(EmptySchemaFound, schemaName), schemaName);
                }
            }
        }

        private IConnectionProvider GetConnectionProvider(IDbConnection connection = null)
        {
            return connection != null ? new ConnectionProvider(connection) as IConnectionProvider
                                      : new DriverConnectionProvider(Driver, ConnectionString);
        }

        private IEnumerable<string> FindSchemas()
        {
            return new List<string>().Union(Schemas ?? new List<string>())
                                     .Union(new List<string> { MetadataTableSchema })
                                     .Where(s => !s.IsNullOrWhiteSpace())
                                     .Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}
