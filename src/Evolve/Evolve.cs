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
    public class Evolve : IEvolveConfiguration
    {
        #region Fields

        private const string IncorrectChecksum = "Checksum validation failed for script: {0}.";
        private const string MigrationMetadataNotFound = "Script {0} not found in the metadata table of applied migrations.";
        private const string NewSchemaCreated = "Create new schema: {0}.";
        private const string EmptySchemaFound = "Empty schema found: {0}.";
        private const string ScriptMigrationError = "Error executing script: {0}.";

        private string _configurationPath;
        private IDbConnection _userDbConnection;
        private IMigrationLoader _loader = new FileMigrationLoader();

        #endregion

        /// <summary>
        ///     <para>
        ///         Constructor.
        ///     </para>
        ///     <para>
        ///         Set the default configuration values.
        ///     </para>
        ///     <para>
        ///         Load the configuration file at <paramref name="evolveConfigurationPath"/>.
        ///     </para>
        /// </summary>
        /// <param name="evolveConfigurationPath"></param>
        /// <param name="dbConnection"></param>
        public Evolve(string evolveConfigurationPath, IDbConnection dbConnection = null)
        {
            _configurationPath = Check.FileExists(evolveConfigurationPath, nameof(evolveConfigurationPath));
            _userDbConnection = dbConnection;

            // Set default configuration settings
            Command = CommandOptions.Migrate;
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
            TargetVersion = new MigrationVersion(long.MaxValue.ToString());

            // Configure Evolve
            var configurationProvider = ConfigurationFactoryProvider.GetProvider(evolveConfigurationPath);
            configurationProvider.Configure(evolveConfigurationPath, this);
        }

        #region IEvolveConfiguration

        public string ConnectionString { get; set; }
        public IEnumerable<string> Schemas { get; set; }
        public string Driver { get; set; }
        public CommandOptions Command { get; set; }
        public bool IsEraseDisabled { get; set; }
        public bool MustEraseOnValidationError { get; set; }
        public Encoding Encoding { get; set; }
        public IEnumerable<string> Locations { get; set; }
        public string MetadaTableName { get; set; }

        private string _metadaTableSchema;
        public string MetadataTableSchema
        {
            get => _metadaTableSchema.IsNullOrWhiteSpace() ? Schemas?.FirstOrDefault() : _metadaTableSchema;
            set => _metadaTableSchema = value;
        }

        public string PlaceholderPrefix { get; set; }
        public string PlaceholderSuffix { get; set; }
        public Dictionary<string, string> Placeholders { get; set; }
        public string SqlMigrationPrefix { get; set; }
        public string SqlMigrationSeparator { get; set; }
        public string SqlMigrationSuffix { get; set; }
        public MigrationVersion TargetVersion { get; set; }

        #endregion

        #region Commands

        public void ExecuteCommand()
        {
            switch (Command)
            {
                case CommandOptions.Migrate:
                    Migrate();
                    break;
                case CommandOptions.Repair:
                    Repair();
                    break;
                case CommandOptions.Erase:
                    Erase();
                    break;
                default:
                    Migrate();
                    break;
            }
        }

        public string GenerateScript(string fromMigration = null, string toMigration = null)
        {
            throw new NotImplementedException();
        }

        public void Migrate()
        {
            var db = Initialize();

            try
            {
                Validate(db);
            }
            catch (EvolveValidationException ex)
            {
                if (MustEraseOnValidationError)
                {
                    // TODO: Add LogMessage
                    Erase();
                }
                else
                {
                    throw ex;
                }
            }

            ManageSchemas(db);

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadaTableName);
            var lastAppliedVersion = metadata.GetAllMigrationMetadata().LastOrDefault()?.Version ?? new MigrationVersion("0");
            var scripts = _loader.GetMigrations(Locations, SqlMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix)
                                 .SkipWhile(x => x.Version <= lastAppliedVersion)
                                 .TakeWhile(x => x.Version <= TargetVersion);

            foreach (var script in scripts)
            {
                try
                {
                    db.WrappedConnection.BeginTransaction();
                    db.WrappedConnection.ExecuteNonQuery(script.LoadSQL(Placeholders, Encoding));
                    metadata.SaveMigration(script, true);
                    db.WrappedConnection.Commit();
                }
                catch(Exception ex)
                {
                    db.WrappedConnection.Rollback();
                    metadata.SaveMigration(script, false);
                    throw new EvolveException(string.Format(ScriptMigrationError, script.Name), ex);
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
            if(IsEraseDisabled)
            {
                // TODO: Add LogMessage
                return;
            }

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

        public void Repair()
        {
            var db = Initialize();
            Validate(db);
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
                    throw new EvolveValidationException(string.Format(MigrationMetadataNotFound, script.Name));                       
                }

                string scriptChecksum = script.CalculateChecksum();
                if (scriptChecksum != appliedMigration.Checksum)                                                            // Script found, verify checksum
                {
                    if (Command == CommandOptions.Repair)
                    {
                        metadata.UpdateChecksum(appliedMigration.Id, scriptChecksum);
                    }
                    else
                    {
                        throw new EvolveValidationException(string.Format(IncorrectChecksum, script.Name));
                    }
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
