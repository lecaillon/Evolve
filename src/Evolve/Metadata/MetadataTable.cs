using System;
using System.Collections.Generic;
using System.Linq;
using Evolve.Dialect;
using Evolve.Migration;
using Evolve.Utilities;

namespace Evolve.Metadata
{
    public abstract class MetadataTable : IEvolveMetadata
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

        public void SaveMigration(MigrationScript migration, bool success)
        {
            Check.NotNull(migration, nameof(migration));

            Execute(() =>
            {
                InternalSave(new MigrationMetadata(migration.Version.Label, migration.Description, migration.Name, MetadataType.Migration)
                {
                    Checksum = migration.CalculateChecksum(),
                    Success = success
                });
            });
        }

        public void Save(MetadataType type, string version, string description, string name = "")
        {
            if(type == MetadataType.Migration) throw new ArgumentException(MigrationMetadataTypeNotSupported, nameof(type));

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

        public IEnumerable<MigrationMetadata> GetAllMigrationMetadata()
        {
            return Execute(() =>
            {
                return InternalGetAllMetadata().Where(x => x.Type == MetadataType.Migration && x.Success == true)
                                               .OrderBy(x => x.Version)
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

        public void Lock()
        {
            Execute(() =>
            {
                InternalLock();
            });
        }

        public bool IsExists()
        {
            return Execute(() => InternalIsExists(), false);
        }

        protected abstract bool InternalIsExists();

        protected void Create()
        {
            Execute(() =>
            {
                InternalCreate();
            });
        }

        protected abstract void InternalCreate();

        protected abstract void InternalLock();

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
            bool restoreSchema = false;
            if(!_database.GetCurrentSchemaName().Equals(Schema, StringComparison.OrdinalIgnoreCase))
            {
                _database.ChangeSchema(Schema);
                restoreSchema = true;
            }

            if (createIfNotExists)
            {
                InternalCreateIfNotExists();
            }

            action();

            if(restoreSchema)
            {
                _database.RestoreOriginalSchema();
            }
        }

        private T Execute<T>(Func<T> func, bool createIfNotExists = true)
        {
            bool restoreSchema = false;
            if (!_database.GetCurrentSchemaName().Equals(Schema, StringComparison.OrdinalIgnoreCase))
            {
                _database.ChangeSchema(Schema);
                restoreSchema = true;
            }

            if (createIfNotExists)
            {
                InternalCreateIfNotExists();
            }

            T result = func();

            if (restoreSchema)
            {
                _database.RestoreOriginalSchema();
            }

            return result;
        }
    }
}
