using Evolve.Migration;
using System.Collections.Generic;
using Evolve.Utilities;

namespace Evolve.Metadata
{
    public abstract class MetadataTable : IEvolveMetadata
    {
        public MetadataTable(string schema)
        {
            Schema = Check.NotNullOrEmpty(schema, nameof(schema));
        }

        public string Schema { get; private set; }

        public abstract void Lock();

        public abstract bool CreateIfNotExists();

        public MigrationMetadata AddMigrationMetadata(MigrationScript migration)
        {
            CreateIfNotExists();
            return InternalAddMigrationMetadata(migration);
        }

        public IEnumerable<MigrationMetadata> GetAllMigrationMetadata()
        {
            CreateIfNotExists();
            return InternalGetAllMigrationMetadata();
        }

        protected abstract MigrationMetadata InternalAddMigrationMetadata(MigrationScript migration);

        protected abstract IEnumerable<MigrationMetadata> InternalGetAllMigrationMetadata();
    }
}
