using Evolve.Migration;
using System.Collections.Generic;
using Evolve.Utilities;
using Evolve.Connection;

namespace Evolve.Metadata
{
    public abstract class MetadataTable : IEvolveMetadata
    {
        protected IWrappedConnection _wrappedConnection;

        public MetadataTable(string schema, string tableName, IWrappedConnection wrappedConnection)
        {
            Schema = Check.NotNullOrEmpty(schema, nameof(schema));
            TableName = Check.NotNullOrEmpty(schema, nameof(tableName));
            _wrappedConnection = Check.NotNull(wrappedConnection, nameof(wrappedConnection));
        }

        public string Schema { get; private set; }

        public string TableName { get; private set; }

        public abstract void Lock();

        public abstract bool CreateIfNotExists();

        public void AddMigrationMetadata(MigrationScript migration, bool success)
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

        protected abstract void InternalAddMigrationMetadata(MigrationScript migration, bool success);

        protected abstract IEnumerable<MigrationMetadata> InternalGetAllMigrationMetadata();
    }
}
