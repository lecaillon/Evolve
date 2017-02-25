using Evolve.Connection;
using Evolve.Migration;
using Evolve.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Evolve.Metadata
{
    public abstract class MetadataTable : IEvolveMetadata
    {
        protected const string MigrationMetadataTypeNotSupported = "This method does not support the save of migration metadata. Use SaveMigration() instead.";
        protected readonly WrappedConnection _wrappedConnection;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="schema"> Existing database schema name. </param>
        /// <param name="tableName"> Metadata table name. </param>
        /// <param name="wrappedConnection"> A connection to the database. </param>
        public MetadataTable(string schema, string tableName, WrappedConnection wrappedConnection)
        {
            Schema = Check.NotNullOrEmpty(schema, nameof(schema));
            TableName = Check.NotNullOrEmpty(tableName, nameof(tableName));
            _wrappedConnection = Check.NotNull(wrappedConnection, nameof(wrappedConnection));
        }

        public string Schema { get; }

        public string TableName { get; }

        public bool CreateIfNotExists()
        {
            if (IsExists())
            {
                return false;
            }
            else
            {
                Create();
                return true;
            }
        }

        public void SaveMigration(MigrationScript migration, bool success)
        {
            Check.NotNull(migration, nameof(migration));

            CreateIfNotExists();
            InternalSave(new MigrationMetadata(migration.Version.Label, migration.Description, migration.Name, MetadataType.Migration)
            {
                Checksum = migration.CalculateChecksum(),
                Success = success
            });
        }

        public void Save(MetadataType type, string version, string description, string name = "")
        {
            if(type == MetadataType.Migration) throw new ArgumentException(MigrationMetadataTypeNotSupported, nameof(type));

            Check.NotNullOrEmpty(version, nameof(version));
            Check.NotNullOrEmpty(description, nameof(description));
            Check.NotNull(name, nameof(name));

            CreateIfNotExists();
            InternalSave(new MigrationMetadata(version, description, name, type)
            {
                Checksum = string.Empty,
                Success = true
            });
        }

        public IEnumerable<MigrationMetadata> GetAllMigrationMetadata()
        {
            CreateIfNotExists();
            return InternalGetAllMetadata().Where(x => x.Type == MetadataType.Migration)
                                           .OrderBy(x => x.Version)
                                           .ToList();
        }

        public bool CanDropSchema(string schemaName)
        {
            CreateIfNotExists();
            return InternalGetAllMetadata().Where(x => x.Type == MetadataType.NewSchema && x.Name.Equals(schemaName, StringComparison.OrdinalIgnoreCase))
                                           .Any();
        }

        public bool CanCleanSchema(string schemaName)
        {
            CreateIfNotExists();
            return InternalGetAllMetadata().Where(x => x.Type == MetadataType.EmptySchema && x.Name.Equals(schemaName, StringComparison.OrdinalIgnoreCase))
                                           .Any();
        }

        public MigrationVersion FindStartVersion()
        {
            CreateIfNotExists();
            var metadata = InternalGetAllMetadata().Where(x => x.Type == MetadataType.StartVersion)
                                                   .OrderByDescending(x => x.Version)
                                                   .FirstOrDefault();

            return metadata?.Version ?? new MigrationVersion("0");
        }

        public abstract void Lock();

        public abstract bool IsExists();

        protected abstract void Create();

        protected abstract void InternalSave(MigrationMetadata metadata);

        protected abstract IEnumerable<MigrationMetadata> InternalGetAllMetadata();
    }
}
