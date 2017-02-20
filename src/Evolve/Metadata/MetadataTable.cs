using Evolve.Migration;
using System.Collections.Generic;
using Evolve.Utilities;
using Evolve.Connection;

namespace Evolve.Metadata
{
    public abstract class MetadataTable : IEvolveMetadata
    {
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

        public abstract void Lock();

        public virtual bool CreateIfNotExists()
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

        protected void Save(MigrationMetadata metadata)
        {
            Check.NotNull(metadata, nameof(metadata));

            CreateIfNotExists();
            InternalSave(metadata);
        }

        public void SaveMigrationMetadata(MigrationScript migration, bool success)
        {
            CreateIfNotExists();
            InternalAddMigrationMetadata(migration, success);
        }

        public IEnumerable<MigrationMetadata> GetAllMigrationMetadata()
        {
            CreateIfNotExists();
            return InternalGetAllMigrationMetadata();
        }

        protected abstract bool IsExists();

        protected abstract void Create();

        protected abstract void InternalSave(MigrationMetadata metadata);

        protected abstract void InternalSaveMetadata(MigrationScript migration, bool success);

        protected abstract IEnumerable<MigrationMetadata> InternalGetAllMigrationMetadata();
    }
}
