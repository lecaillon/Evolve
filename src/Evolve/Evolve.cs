﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using ConsoleTables;
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

        private readonly IDbConnection _userCnn;
        private readonly Action<string> _log;

        #endregion

        /// <summary>
        ///     Initialize a new instance of the <see cref="Evolve"/> class.
        /// </summary>
        /// <param name="dbConnection"> The database connection used to apply the migrations. </param>
        /// <param name="logDelegate"> An optional logger. </param>
        public Evolve(IDbConnection dbConnection, Action<string> logDelegate = null)
        {
            _userCnn = Check.NotNull(dbConnection, nameof(dbConnection));
            _log = logDelegate ?? new Action<string>((msg) => { });
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
        public string SqlRepeatableMigrationPrefix { get; set; } = "R";
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
        public long TotalTimeElapsedInMs { get; private set; }
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
                case CommandOptions.Info:
                    Info();
                    break;
                default:
                    _log($"Evolve.Command parameter is not set. No migration applied. See: https://evolve-db.netlify.com/configuration/ for more information.");
                    break;
            }
        }

        public IEnumerable<MigrationMetadata> Info()
        {
            Command = CommandOptions.Info;

            try
            {
                using var db = InitiateDatabaseConnection();
                var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);
                var rows = metadata.GetAllMetadata().ToList();
                var table = new ConsoleTable("Id", "Version", "Category", "Description", "Installed on", "Installed by", "Success", "Checksum")
                    .Configure(o => o.EnableCount = false);
                rows.ForEach(x => table.AddRow(x.Id, x.Version, x.Type, x.Description, x.InstalledOn, x.InstalledBy, x.Success, x.Checksum));
                _log(table.ToStringAlternative());

                return rows;
            }
            catch
            {
                _log("Evolve metadata table cannot be found.");
                return null;
            }
        }

        public void Migrate()
        {
            Command = CommandOptions.Migrate;
            _log("Executing Migrate...");

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
                    _log($"{ex.Message} Erase database. (MustEraseOnValidationError = True)");

                    InternalErase(db);
                    ManageSchemas(db);
                }
                else
                {
                    throw ex;
                }
            }

            if (MigrationLoader.GetMigrations(SqlMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix, Encoding).Count() == 0
             && MigrationLoader.GetRepeatableMigrations(SqlRepeatableMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix, Encoding).Count() == 0)
            {
                _log("No migration script found.");
                return;
            }

            var lastAppliedVersion = ExecuteAllMigration(db);
            ExecuteAllRepeatableMigration(db);

            if (NbMigration == 0)
            {
                _log("Database is up to date. No migration needed.");
            }
            else
            {
                _log($"Database migrated to version {lastAppliedVersion}. {NbMigration} migration(s) applied in {TotalTimeElapsedInMs} ms.");
            }
        }

        /// <summary>
        ///     Execute new versioned migrations considering <see cref="StartVersion"/> and <see cref="TargetVersion"/>.
        /// </summary>
        /// <returns> The version of the last applied versioned migration or <see cref="MigrationVersion.MinVersion"/> if none. </returns>
        private MigrationVersion ExecuteAllMigration(DatabaseHelper db)
        {
            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);
            var lastAppliedVersion = metadata.GetAllMigrationMetadata().LastOrDefault()?.Version ?? MigrationVersion.MinVersion;
            var startVersion = metadata.FindStartVersion(); // Load start version from metadata
            var migrations = MigrationLoader.GetMigrations(SqlMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix, Encoding)
                                            .SkipWhile(x => x.Version < startVersion)
                                            .SkipWhile(x => x.Version <= lastAppliedVersion)
                                            .TakeWhile(x => x.Version <= TargetVersion);

            foreach (var migration in migrations)
            {
                ExecuteMigration(migration, db);
            }

            return migrations.Any() 
                ? migrations.Last().Version 
                : lastAppliedVersion;
        }

        /// <summary>
        ///     Execute new repeatable migrations and all those for which the checksum has changed since the last execution.
        /// </summary>
        private void ExecuteAllRepeatableMigration(DatabaseHelper db)
        {
            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);
            var appliedMigrations = metadata.GetAllRepeatableMigrationMetadata();
            var migrations = MigrationLoader.GetRepeatableMigrations(SqlRepeatableMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix, Encoding);
            foreach (var migration in migrations)
            {
                var appliedMigration = appliedMigrations.Where(x => x.Name == migration.Name)
                                                        .OrderBy(x => x.InstalledOn)
                                                        .LastOrDefault();
                if (appliedMigration is null
                 || appliedMigration.Checksum != migration.CalculateChecksum())
                {
                    ExecuteMigration(migration, db);
                }
            }
        }

        public void Repair()
        {
            Command = CommandOptions.Repair;
            _log("Executing Repair...");

            InternalExecuteCommand(db =>
            {
                ValidateAndRepairMetadata(db);

                if (NbReparation == 0)
                {
                    _log("Metadata are up to date. Repair cancelled.");
                }
                else
                {
                    _log($"Successfully repaired {NbReparation} migration(s).");
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
            _log("Executing Erase...");

            if (IsEraseDisabled)
            {
                throw new EvolveConfigurationException("Erase is disabled.");
            }

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);

            if (!metadata.IsExists())
            {
                _log("No metadata found. Erase cancelled.");
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
                        _log($"Successfully dropped schema {schemaName}.");
                        NbSchemaErased++;
                    }
                    catch (Exception ex)
                    {
                        throw new EvolveException($"Erase failed. Impossible to drop schema {schemaName}.", ex);
                    }
                }
                else if (metadata.CanEraseSchema(schemaName))
                {
                    try
                    {
                        db.GetSchema(schemaName).Erase();
                        _log($"Successfully erased schema {schemaName}.");
                        NbSchemaErased++;
                    }
                    catch (Exception ex)
                    {
                        throw new EvolveException($"Erase failed. Impossible to erase schema {schemaName}.", ex);
                    }
                }
                else
                {
                    _log($"Cannot erase schema {schemaName}. This schema was not empty when Evolve first started migrations.");
                    NbSchemaToEraseSkipped++;
                }
            }

            if (!db.WrappedConnection.CassandraCluster)
            {
                db.WrappedConnection.TryCommit();
            }

            _log($"Erase schema(s) completed: {NbSchemaErased} erased, {NbSchemaToEraseSkipped} skipped.");
        }

        #endregion

        private void InternalExecuteCommand(Action<DatabaseHelper> commandAction)
        {
            NbMigration = 0;
            NbReparation = 0;
            NbSchemaErased = 0;
            NbSchemaToEraseSkipped = 0;
            TotalTimeElapsedInMs = 0;

            using var db = InitiateDatabaseConnection();

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
                        _log("Error trying to release Evolve lock.");
                    }
                }
            }
        }

        private void ExecuteMigration(MigrationScript migration, DatabaseHelper db)
        {
            Check.NotNull(migration, nameof(migration));
            Check.NotNull(db, nameof(db));

            var stopWatch = new Stopwatch();
            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);

            try
            {
                stopWatch.Start();
                foreach (var statement in db.SqlStatementBuilder.LoadSqlStatements(migration, Placeholders))
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

                metadata.SaveMigration(migration, true);
                db.WrappedConnection.TryCommit();
                stopWatch.Stop();

                _log($"Successfully applied migration {migration.Name} in {stopWatch.ElapsedMilliseconds} ms.");
                TotalTimeElapsedInMs += stopWatch.ElapsedMilliseconds;
                NbMigration++;
            }
            catch (Exception ex)
            {
                stopWatch.Stop();
                TotalTimeElapsedInMs += stopWatch.ElapsedMilliseconds;
                db.WrappedConnection.TryRollback();
                metadata.SaveMigration(migration, false);
                throw new EvolveException($"Error executing script: {migration.Name} after {stopWatch.ElapsedMilliseconds} ms.", ex);
            }
        }

        private DatabaseHelper InitiateDatabaseConnection()
        {
            var evolveCnn = new WrappedConnection(_userCnn).Validate();
            var dbmsType = evolveCnn.GetDatabaseServerType();
            var db = DatabaseHelperFactory.GetDatabaseHelper(dbmsType, evolveCnn);

            if (Schemas is null || Schemas.Count() == 0)
            { // If no schema, get the one associated to the datasource connection
                Schemas = new[] { db.GetCurrentSchemaName() };
            }

            if (Command != CommandOptions.Info)
            {
                _log("Evolve initialized.");
            }

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

                _log("Cannot acquire Evolve application lock. Another migration is running.");
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

                _log("Cannot acquire Evolve table lock. Another migration is running.");
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
                    _log($"Schema {schemaName} does not exist.");

                    // Create new schema
                    schema.Create();
                    metadata.Save(MetadataType.NewSchema, "0", $"Create new schema: {schemaName}.", schemaName);

                    _log($"Schema {schemaName} created.");
                }
                else if (schema.IsEmpty())
                {
                    // Mark schema as empty in the metadata table
                    metadata.Save(MetadataType.EmptySchema, "0", $"Empty schema found: {schemaName}.", schemaName);

                    _log($"Mark schema {schemaName} as empty.");
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
                throw new EvolveConfigurationException($"The database has already been flagged with a StartVersion ({currentStartVersion}). Only one StartVersion parameter is allowed.");
            }

            if (metadata.GetAllMigrationMetadata().Any())
            { // At least one migration has already been applied, StartVersion parameter not allowed anymore
                throw new EvolveConfigurationException("Use of the StartVersion parameter is not allowed when migrations have already been applied.");
            }

            // Apply StartVersion parameter
            metadata.Save(MetadataType.StartVersion, StartVersion.Label, $"Skip migrations until version {StartVersion.Label} excluded.", $"StartVersion = {StartVersion.Label}");
        }

        private void ValidateAndRepairMetadata(DatabaseHelper db)
        {
            Check.NotNull(db, nameof(db));

            var metadata = db.GetMetadataTable(MetadataTableSchema, MetadataTableName);
            if (!metadata.IsExists())
            { // Nothing to validate
                _log("No metadata found.");
                return;
            }

            var appliedMigrations = metadata.GetAllMigrationMetadata(); // Load all applied migrations metadata
            if (appliedMigrations.Count() == 0)
            { // Nothing to validate
                _log("No metadata found.");
                return;
            }

            var lastAppliedVersion = appliedMigrations.Last().Version; // Get the last applied migration version
            var startVersion = metadata.FindStartVersion(); // Load start version from metadata
            var migrations = MigrationLoader.GetMigrations(SqlMigrationPrefix, SqlMigrationSeparator, SqlMigrationSuffix, Encoding)
                                            .SkipWhile(x => x.Version < startVersion)
                                            .TakeWhile(x => x.Version <= lastAppliedVersion); // Keep scripts between first and last applied migration

            foreach (var migration in migrations)
            { // Search script in the applied migrations
                var appliedMigration = appliedMigrations.SingleOrDefault(x => x.Version == migration.Version);
                if (appliedMigration == null)
                { // Script not found
                    if (Command == CommandOptions.Migrate && OutOfOrder)
                    { // Apply migration
                        ExecuteMigration(migration, db);
                        continue;
                    }
                    else
                    { // Validation error
                        throw new EvolveValidationException($"Validation failed: script {migration.Name} not found in the metadata table of applied migrations.");
                    }
                }

                try
                { // Script found, verify checksum
                    migration.ValidateChecksum(appliedMigration.Checksum);
                }
                catch (Exception ex)
                { // Validation error
                    if (Command == CommandOptions.Repair)
                    { // Repair by updating checksum
                        metadata.UpdateChecksum(appliedMigration.Id, migration.CalculateChecksum());
                        NbReparation++;

                        _log($"Checksum fixed for migration: {migration.Name}.");
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            _log("Metadata validated.");
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