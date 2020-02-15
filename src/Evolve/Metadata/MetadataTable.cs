﻿using System;
using System.Collections.Generic;
using System.Linq;
using Evolve.Dialect;
using Evolve.Migration;
using Evolve.Utilities;

namespace Evolve.Metadata
{
    internal abstract class MetadataTable : IEvolveMetadata
    {
        protected const string MigrationMetadataTypeNotSupported = "This method does not support the save of migration metadata. Use SaveMigration() instead.";
        protected readonly DatabaseHelper _database;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="schema"> Existing database schema name. </param>
        /// <param name="tableName"> Metadata table name. </param>
        /// <param name="database"> A database helper used to change and restore schema of the metadata table. </param>
        public MetadataTable(string schema, string tableName, DatabaseHelper database)
        {
            Schema = Check.NotNullOrEmpty(schema, nameof(schema));
            TableName = Check.NotNullOrEmpty(tableName, nameof(tableName));
            _database = Check.NotNull(database, nameof(database));
        }

        public string Schema { get; }

        public string TableName { get; }

        public bool CreateIfNotExists()
        {
            return Execute(() => InternalCreateIfNotExists(), false);
        }

        public void SaveMigration(MigrationScript migration, bool success, TimeSpan? elapsed = null)
        {
            Check.NotNull(migration, nameof(migration));

            Execute(() =>
            {
                string description = elapsed is null
                    ? migration.Description 
                    : $"{migration.Description} ({Math.Round(elapsed.Value.TotalMilliseconds)} ms)";

                InternalSave(new MigrationMetadata(migration.Version?.Label, description, migration.Name, migration.Type)
                {
                    Checksum = migration.CalculateChecksum(),
                    Success = success
                });
            });
        }

        public void Save(MetadataType type, string version, string description, string name = "")
        {
            if (type == MetadataType.Migration || type == MetadataType.RepeatableMigration)
            {
                throw new ArgumentException(MigrationMetadataTypeNotSupported, nameof(type));
            }

            Check.NotNullOrEmpty(version, nameof(version));
            Check.NotNullOrEmpty(description, nameof(description));
            Check.NotNull(name, nameof(name));

            Execute(() =>
            {
                InternalSave(new MigrationMetadata(version, description, name, type)
                {
                    Checksum = string.Empty,
                    Success = true
                });
            });
        }

        public void UpdateChecksum(int migrationId, string checksum)
        {
            Check.NotNullOrEmpty(checksum, nameof(checksum));
            if (migrationId < 1) throw new ArgumentOutOfRangeException(nameof(migrationId), nameof(migrationId) + " must be positive.");

            Execute(() =>
            {
                InternalUpdateChecksum(migrationId, checksum);
            });
        }

        public IEnumerable<MigrationMetadata> GetAllMetadata() => Execute(() => InternalGetAllMetadata(), createIfNotExists: false);

        public IEnumerable<MigrationMetadata> GetAllAppliedMigration()
        {
            return Execute(() =>
            {
                return InternalGetAllMetadata().Where(x => x.Type == MetadataType.Migration && x.Success == true)
                                               .OrderBy(x => x.Version)
                                               .ToList();
            });
        }

        public MigrationVersion FindLastAppliedVersion()
        {
            return Execute(() =>
            {
                return GetAllAppliedMigration().OrderBy(x => x.Version).LastOrDefault()?.Version ?? MigrationVersion.MinVersion;
            });
        }

        public IEnumerable<MigrationMetadata> GetAllAppliedRepeatableMigration()
        {
            return Execute(() =>
            {
                return InternalGetAllMetadata().Where(x => x.Type == MetadataType.RepeatableMigration && x.Success == true)
                                               .OrderBy(x => x.Name)
                                               .ToList();
            });
        }

        public bool CanDropSchema(string schemaName)
        {
            return Execute(() =>
            {
                return InternalGetAllMetadata().Where(x => x.Type == MetadataType.NewSchema && x.Name.Equals(schemaName, StringComparison.OrdinalIgnoreCase))
                                               .Any();
            });
        }

        public bool CanEraseSchema(string schemaName)
        {
            return Execute(() =>
            {
                return InternalGetAllMetadata().Where(x => x.Type == MetadataType.EmptySchema && x.Name.Equals(schemaName, StringComparison.OrdinalIgnoreCase))
                                               .Any();
            });
        }

        public MigrationVersion FindStartVersion()
        {
            return Execute(() =>
            {
                var metadata = InternalGetAllMetadata().Where(x => x.Type == MetadataType.StartVersion)
                                                       .OrderByDescending(x => x.Version)
                                                       .FirstOrDefault();

                return metadata?.Version ?? MigrationVersion.MinVersion;
            });
        }

        public bool TryLock() => Execute(() => InternalTryLock());

        public bool ReleaseLock()
        {
            if (!InternalIsExists())
            { // The metadatatable does not exist, so neither the lock
                return true;
            }

            return Execute(() => InternalReleaseLock(), createIfNotExists: false);
        }

        public bool IsExists() => Execute(() => InternalIsExists(), createIfNotExists: false);

        protected abstract bool InternalIsExists();

        protected void Create()
        {
            Execute(() =>
            {
                InternalCreate();
            });
        }

        protected abstract void InternalCreate();

        protected abstract bool InternalTryLock();

        protected abstract bool InternalReleaseLock();

        protected abstract void InternalSave(MigrationMetadata metadata);

        protected abstract void InternalUpdateChecksum(int migrationId, string checksum);

        protected abstract IEnumerable<MigrationMetadata> InternalGetAllMetadata();

        private bool InternalCreateIfNotExists()
        {
            if (InternalIsExists())
            {
                return false;
            }
            else
            {
                InternalCreate();
                return true;
            }
        }

        private void Execute(Action action, bool createIfNotExists = true)
        {
            if (createIfNotExists)
            {
                InternalCreateIfNotExists();
            }

            action();
        }

        private T Execute<T>(Func<T> func, bool createIfNotExists = true)
        {
            if (createIfNotExists)
            {
                InternalCreateIfNotExists();
            }

            T result = func();

            return result;
        }
    }
}
