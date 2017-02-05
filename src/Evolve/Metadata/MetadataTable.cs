using Evolve.Migration;
using System.Collections.Generic;
using Evolve.Utilities;
using Evolve.Connection;

namespace Evolve.Metadata
{
    public abstract class MetadataTable : IEvolveMetadata
    {
        protected IWrappedConnection _wrappedConnection;

        public MetadataTable(string schema, string name, IWrappedConnection wrappedConnection)
        {
            Schema = Check.NotNullOrEmpty(schema, nameof(schema));
            Name = Check.NotNullOrEmpty(schema, nameof(name));
            _wrappedConnection = Check.NotNull(wrappedConnection, nameof(wrappedConnection));
        }

        public string Schema { get; private set; }

        public string Name { get; private set; }

        public abstract void Lock();

        public abstract bool CreateIfNotExists();

        public MigrationMetadata AddMigrationMetadata(MigrationScript migration, bool success)
        {
            CreateIfNotExists();
            return InternalAddMigrationMetadata(migration, success);
        }

        public IEnumerable<MigrationMetadata> GetAllMigrationMetadata()
        {
            CreateIfNotExists();
            return InternalGetAllMigrationMetadata();
        }

        protected abstract bool IsExists();

        protected abstract void Create();

        protected abstract MigrationMetadata InternalAddMigrationMetadata(MigrationScript migration, bool success);

        protected abstract IEnumerable<MigrationMetadata> InternalGetAllMigrationMetadata();
    }
}
