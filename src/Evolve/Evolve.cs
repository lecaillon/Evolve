using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
        #region Constants

        // Initialize
        private const string InvalidConfigurationLocation = "Evolve configuration file not found at: {0}.";
        private const string EvolveInitialized = "Evolve initialized.";
        private const string NoCommandSpecified = "Evolve.Command parameter is not set. No migration applied. See: https://evolve-db.netlify.com/configuration/ for more information.";
        private const string CannotAcquireApplicationLock = "Cannot acquire Evolve application lock. Another migration is running.";
        private const string CannotAcquireMetadatatableLock = "Cannot acquire Evolve table lock. Another migration is running.";
        private const string CannotReleaseLock = "Error trying to release Evolve lock.";

        // Validate
        private const string MigrationMetadataNotFound = "Validate failed: script {0} not found in the metadata table of applied migrations.";
        private const string ChecksumFixed = "Checksum fixed for migration: {0}.";
        private const string NoMetadataFound = "No metadata found.";
        private const string ValidateSuccessfull = "Metadata validated.";

        // ManageSchemas
        private const string NewSchemaCreated = "Create new schema: {0}.";
        private const string EmptySchemaFound = "Empty schema found: {0}.";
        private const string SchemaNotExists = "Schema {0} does not exist.";
        private const string SchemaCreated = "Schema {0} created.";
        private const string SchemaMarkedEmpty = "Mark schema {0} as empty.";

        // ManageStartVersion
        private const string MultipleStartVersionError = "The database has already been flagged with a StartVersion ({0}). Only one StartVersion parameter is allowed.";
        private const string StartVersionMetadataDesc = "Skip migration scripts until version {0} excluded.";
        private const string StartVersionMetadataName = "StartVersion = {0}";
        private const string StartVersionNotAllowed = "Use of the StartVersion parameter is not allowed when migrations have already been applied.";

        // Migrate
        private const string ExecutingMigrate = "Executing Migrate...";
        private const string MigrationError = "Error executing script: {0}.";
        private const string MigrationErrorEraseOnValidationError = "{0} Erase database. (MustEraseOnValidationError = True)";
        private const string MigrationSuccessfull = "Successfully applied migration {0}.";
        private const string NoMigrationScript = "No migration script found.";
        private const string NothingToMigrate = "Database is up to date. No migration needed.";
        private const string MigrateSuccessfull = "Database migrated to version {0}. {1} migration(s) applied.";
        private const string MigrateOutOfOrderSuccessfull = "{0} out of order migration(s) applied.";

        // Erase
        private const string ExecutingErase = "Executing Erase...";
        private const string EraseDisabled = "Erase is disabled.";
        private const string EraseCancelled = "No metadata found. Erase cancelled.";
        private const string EraseSchemaSuccessfull = "Successfully erased schema {0}.";
        private const string DropSchemaSuccessfull = "Successfully dropped schema {0}.";
        private const string EraseSchemaImpossible = "Cannot erase schema {0}. This schema was not empty when Evolve first started migrations.";
        private const string EraseSchemaFailed = "Erase failed. Impossible to erase schema {0}.";
        private const string DropSchemaFailed = "Erase failed. Impossible to drop schema {0}.";
        private const string EraseCompleted = "Erase schema(s) completed: {0} erased, {1} skipped.";

        // Repair
        private const string ExecutingRepair = "Executing Repair...";
        private const string RepairSuccessfull = "Successfully repaired {0} migration(s).";
        private const string RepairCancelled = "Metadata are up to date. Repair cancelled.";

        #endregion

        #region Fields

        private readonly IDbConnection _userDbConnection;
        private readonly Action<string> _logInfoDelegate;

        #endregion

        /// <summary>
        ///     Initialize a new instance of the <see cref="Evolve"/> class.
        /// </summary>
        /// <param name="dbConnection"> The database connection used to apply the migrations. </param>
        /// <param name="logInfoDelegate"> An optional logger. </param>
        public Evolve(IDbConnection dbConnection, Action<string> logInfoDelegate = null)
        {
            _userDbConnection = Check.NotNull(dbConnection, nameof(dbConnection));
            _logInfoDelegate = logInfoDelegate ?? new Action<string>((msg) => { });
        }

        #region IEvolveConfiguration

        public IEnumerable<string> Schemas { get; set; } = new List<string>();
        public CommandOptions Command { get; set; } = CommandOptions.DoNothing;
        public bool IsEraseDisabled { get; set; }
        public bool MustEraseOnValidationError { get; set; }
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public IEnumerable<string> Locations { get; set; } = new List<string> { "Sql_Scripts" };
        public string MetadataTableName { get; set; } = "changelog";

        private string _metadaTableSchema;
        public string MetadataTableSchema
        {
            get => _metadaTableSchema.IsNullOrWhiteSpace() ? Schemas?.FirstOrDefault() : _metadaTableSchema;
            set => _metadaTableSchema = value;
        }
        public string PlaceholderPrefix { get; set; } = "${";
        public string PlaceholderSuffix { get; set; } = "}";
        public Dictionary<string, string> Placeholders { get; set; } = new Dictionary<string, string>();
        public string SqlMigrationPrefix { get; set; } = "V";
        public string SqlMigrationSeparator { get; set; } = "__";
        public string SqlMigrationSuffix { get; set; } = ".sql";
        public MigrationVersion TargetVersion { get; set; } = MigrationVersion.MaxVersion;
        public MigrationVersion StartVersion { get; set; } = MigrationVersion.MinVersion;
        public bool EnableClusterMode { get; set; } = true;
        public bool OutOfOrder { get; set; } = false;
        public int? CommandTimeout { get; set; }
        public IEnumerable<Assembly> EmbeddedResourceAssemblies { get; set; } = new List<Assembly>();
        public IEnumerable<string> EmbeddedResourceFilters { get; set; } = new List<string>();

        #endregion

        #region Properties

        public int NbMigration { get; private set; }
        public int NbReparation { get; private set; }
        public int NbSchemaErased { get; private set; }
        public int NbSchemaToEraseSkipped { get; private set; }
        public IMigrationLoader MigrationLoader
        {
            get => EmbeddedResourceAssemblies.Any() ? new EmbeddedResourceMigrationLoader(EmbeddedResourceAssemblies, EmbeddedResourceFilters)
                                                  : new FileMigrationLoader(Locations) as IMigrationLoader;
        }

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
                    _logInfoDelegate(NoCommandSpecified);
                    break;
            }
        }

        public void Migrate()
        {
            Command = CommandOptions.Migrate;
            _logInfoDelegate(ExecutingMigrate);

            InternalExecuteCommand(db =>
            {
                InternalMigrate(db);
            });
        }

        private void InternalMigrate(DatabaseHelper db)
        {
            try
            {
                ValidateAndRepairMetadata(db);
            }
            catch (EvolveValidationException ex)
            {
                if (MustEraseOnValidationError)
                {
                    _logInfoDelegate(string.Format(MigrationErrorEraseOnValidationError, ex.Message));

                    InternalErase(db);
                    ManageSchemas(db);
                }
                else
                {
                    throw ex;
                }
            }

            if (MigrationLoader.GetMigrations(SqlMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix, Encoding).Count() == 0)
            {
                _logInfoDelegate(NoMigrationScript);
                return;
            }

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);
            var lastAppliedVersion = metadata.GetAllMigrationMetadata().LastOrDefault()?.Version ?? MigrationVersion.MinVersion;
            var startVersion = metadata.FindStartVersion(); // Load start version from metadata
            var scripts = MigrationLoader.GetMigrations(SqlMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix, Encoding)
                                         .SkipWhile(x => x.Version < startVersion)
                                         .SkipWhile(x => x.Version <= lastAppliedVersion)
                                         .TakeWhile(x => x.Version <= TargetVersion);

            foreach (var script in scripts)
            {
                ExecuteMigrationScript(script, db);
            }

            if (NbMigration == 0)
            {
                _logInfoDelegate(NothingToMigrate);
            }
            else
            {
                if (scripts.Count() == 0)
                {
                    _logInfoDelegate(string.Format(MigrateOutOfOrderSuccessfull, NbMigration));
                }
                else
                {
                    _logInfoDelegate(string.Format(MigrateSuccessfull, scripts.Last().Version, NbMigration));
                }
            }
        }

        public void Repair()
        {
            Command = CommandOptions.Repair;
            _logInfoDelegate(ExecutingRepair);

            InternalExecuteCommand(db =>
            {
                ValidateAndRepairMetadata(db);

                if (NbReparation == 0)
                {
                    _logInfoDelegate(RepairCancelled);
                }
                else
                {
                    _logInfoDelegate(string.Format(RepairSuccessfull, NbReparation));
                }
            });
        }

        public void Erase()
        {
            Command = CommandOptions.Erase;

            InternalExecuteCommand(db =>
            {
                InternalErase(db);
            });
        }

        private void InternalErase(DatabaseHelper db)
        {
            _logInfoDelegate(ExecutingErase);

            if (IsEraseDisabled)
            {
                throw new EvolveConfigurationException(EraseDisabled);
            }

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);

            if (!metadata.IsExists())
            {
                _logInfoDelegate(EraseCancelled);
                return;
            }

            if (!db.WrappedConnection.CassandraCluster)
            {
                db.WrappedConnection.TryBeginTransaction();
            }

            foreach (var schemaName in FindSchemas().Reverse())
            {
                if (metadata.CanDropSchema(schemaName))
                {
                    try
                    {
                        db.GetSchema(schemaName).Drop();
                        _logInfoDelegate(string.Format(DropSchemaSuccessfull, schemaName));
                        NbSchemaErased++;
                    }
                    catch (Exception ex)
                    {
                        throw new EvolveException(string.Format(DropSchemaFailed, schemaName), ex);
                    }
                }
                else if (metadata.CanEraseSchema(schemaName))
                {
                    try
                    {
                        db.GetSchema(schemaName).Erase();
                        _logInfoDelegate(string.Format(EraseSchemaSuccessfull, schemaName));
                        NbSchemaErased++;
                    }
                    catch (Exception ex)
                    {
                        throw new EvolveException(string.Format(EraseSchemaFailed, schemaName), ex);
                    }
                }
                else
                {
                    _logInfoDelegate(string.Format(EraseSchemaImpossible, schemaName));
                    NbSchemaToEraseSkipped++;
                }
            }

            if (!db.WrappedConnection.CassandraCluster)
            {
                db.WrappedConnection.TryCommit();
            }

            _logInfoDelegate(string.Format(EraseCompleted, NbSchemaErased, NbSchemaToEraseSkipped));
        }

        #endregion

        private void InternalExecuteCommand(Action<DatabaseHelper> commandAction)
        {
            NbMigration = 0;
            NbReparation = 0;
            NbSchemaErased = 0;
            NbSchemaToEraseSkipped = 0;

            var db = InitiateDatabaseConnection();

            if (EnableClusterMode)
            {
                WaitForApplicationLock(db);
            }

            try
            {
                ManageSchemas(db); // Ensures all schema are created before using the metadatatable

                if (EnableClusterMode)
                {
                    WaitForMetadataTableLock(db);
                }

                ManageStartVersion(db);

                commandAction(db);
            }
            finally
            {
                if (EnableClusterMode)
                {
                    var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);
                    if (!db.ReleaseApplicationLock() || !metadata.ReleaseLock())
                    {
                        _logInfoDelegate(CannotReleaseLock);
                    }
                }

                db.CloseConnection();
            }
        }

        private void ExecuteMigrationScript(MigrationScript script, DatabaseHelper db)
        {
            Check.NotNull(script, nameof(script));
            Check.NotNull(db, nameof(db));

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);

            try
            {
                foreach (var statement in db.SqlStatementBuilder.LoadSqlStatements(script, Placeholders))
                {
                    if (statement.MustExecuteInTransaction)
                    {
                        db.WrappedConnection.TryBeginTransaction();
                    }
                    else
                    {
                        db.WrappedConnection.TryCommit();
                    }

                    db.WrappedConnection.ExecuteNonQuery(statement.Sql, CommandTimeout);
                }

                metadata.SaveMigration(script, true);
                db.WrappedConnection.TryCommit();

                _logInfoDelegate(string.Format(MigrationSuccessfull, script.Name));
                NbMigration++;
            }
            catch (Exception ex)
            {
                db.WrappedConnection.TryRollback();
                metadata.SaveMigration(script, false);
                throw new EvolveException(string.Format(MigrationError, script.Name), ex);
            }
        }

        private DatabaseHelper InitiateDatabaseConnection()
        {
            var connectionProvider = new ConnectionProvider(_userDbConnection);             // Get a database connection provider
            var evolveConnection = connectionProvider.GetConnection();                      // Get a connection to the database
            evolveConnection.Validate();                                                    // Validate the reliabilty of the initiated connection
            var dbmsType = evolveConnection.GetDatabaseServerType();                        // Get the DBMS type
            var db = DatabaseHelperFactory.GetDatabaseHelper(dbmsType, evolveConnection);   // Get the DatabaseHelper
            if (Schemas == null || Schemas.Count() == 0)
            {
                Schemas = new List<string> { db.GetCurrentSchemaName() };                   // If no schema, get the one associated to the datasource connection
            }

            _logInfoDelegate(EvolveInitialized);

            return db;
        }

        private void WaitForApplicationLock(DatabaseHelper db)
        {
            while (true)
            {
                if (db.TryAcquireApplicationLock())
                {
                    break;
                }

                _logInfoDelegate(CannotAcquireApplicationLock);
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }

        private void WaitForMetadataTableLock(DatabaseHelper db)
        {
            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);

            while (true)
            {
                if (metadata.TryLock())
                {
                    break;
                }

                _logInfoDelegate(CannotAcquireMetadatatableLock);
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }

        private void ManageSchemas(DatabaseHelper db)
        {
            Check.NotNull(db, nameof(db));

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);

            foreach (var schemaName in FindSchemas())
            {
                var schema = db.GetSchema(schemaName);

                if (!schema.IsExists())
                {
                    _logInfoDelegate(string.Format(SchemaNotExists, schemaName));

                    // Create new schema
                    db.WrappedConnection.BeginTransaction();
                    schema.Create();
                    metadata.Save(MetadataType.NewSchema, "0", string.Format(NewSchemaCreated, schemaName), schemaName);
                    db.WrappedConnection.Commit();

                    _logInfoDelegate(string.Format(SchemaCreated, schemaName));
                }
                else if (schema.IsEmpty())
                {
                    // Mark schema as empty in the metadata table
                    metadata.Save(MetadataType.EmptySchema, "0", string.Format(EmptySchemaFound, schemaName), schemaName);

                    _logInfoDelegate(string.Format(SchemaMarkedEmpty, schemaName));
                }
            }
        }

        private void ManageStartVersion(DatabaseHelper db)
        {
            if (StartVersion == null || StartVersion == MigrationVersion.MinVersion)
            { // StartVersion parameter undefined
                return;
            }

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);
            var currentStartVersion = metadata.FindStartVersion();

            if (currentStartVersion == StartVersion)
            { // The StartVersion parameter has already been applied
                return;
            }

            if (currentStartVersion != MigrationVersion.MinVersion)
            { // Metadatatable StartVersion found and do not match the StartVersion parameter
                throw new EvolveConfigurationException(string.Format(MultipleStartVersionError, currentStartVersion));
            }

            if (metadata.GetAllMigrationMetadata().Any())
            { // At least one migration has already been applied, StartVersion parameter not allowed anymore
                throw new EvolveConfigurationException(StartVersionNotAllowed);
            }

            // Apply StartVersion parameter
            metadata.Save(MetadataType.StartVersion, StartVersion.Label, string.Format(StartVersionMetadataDesc, StartVersion.Label), string.Format(StartVersionMetadataName, StartVersion.Label));
        }

        private void ValidateAndRepairMetadata(DatabaseHelper db)
        {
            Check.NotNull(db, nameof(db));

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);                                       // Get the metadata table
            if (!metadata.IsExists())
            {
                _logInfoDelegate(NoMetadataFound); // Nothing to validate
                return;
            }

            var appliedMigrations = metadata.GetAllMigrationMetadata();                                                     // Load all applied migrations metadata
            if (appliedMigrations.Count() == 0)
            {
                _logInfoDelegate(NoMetadataFound); // Nothing to validate
                return;
            }

            var lastAppliedVersion = appliedMigrations.Last().Version;                                                      // Get the last applied migration version
            var startVersion = metadata.FindStartVersion();                                                                 // Load start version from metadata
            var scripts = MigrationLoader.GetMigrations(SqlMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix, Encoding)
                                         .SkipWhile(x => x.Version < startVersion)
                                         .TakeWhile(x => x.Version <= lastAppliedVersion);                                  // Keep scripts between first and last applied migration

            foreach (var script in scripts)
            {
                var appliedMigration = appliedMigrations.SingleOrDefault(x => x.Version == script.Version);                 // Search script in the applied migrations
                if (appliedMigration == null)
                {
                    if (Command == CommandOptions.Migrate && OutOfOrder)
                    {
                        ExecuteMigrationScript(script, db);
                        continue;
                    }
                    else
                    {
                        throw new EvolveValidationException(string.Format(MigrationMetadataNotFound, script.Name));
                    }
                }

                try
                {
                    script.ValidateChecksum(appliedMigration.Checksum);                                                     // Script found, verify checksum
                }
                catch (Exception ex)
                {
                    if (Command == CommandOptions.Repair)
                    {
                        metadata.UpdateChecksum(appliedMigration.Id, script.CalculateChecksum());
                        NbReparation++;

                        _logInfoDelegate(string.Format(ChecksumFixed, script.Name));
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            _logInfoDelegate(ValidateSuccessfull);
        }

        private IEnumerable<string> FindSchemas()
        {
            return new List<string>().Union(new List<string> { MetadataTableSchema ?? string.Empty })
                                     .Union(Schemas ?? new List<string>())
                                     .Where(s => !s.IsNullOrWhiteSpace())
                                     .Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}