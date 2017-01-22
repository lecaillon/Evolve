using Evolve.Migration;
using System.Collections.Generic;

namespace Evolve.Metadata
{
    public abstract class MetadataTable : IEvolveMetadata
    {
        public MigrationMetadata AddMigrationMetadata(MigrationScript migration)
        {
            CreateTableIfNotExists();
            return InternalAddMigrationMetadata(migration);
        }

        public IEnumerable<MigrationMetadata> GetAllMigrationMetadata()
        {
            CreateTableIfNotExists();
            return InternalGetAllMigrationMetadata();
        }

        protected abstract MigrationMetadata InternalAddMigrationMetadata(MigrationScript migration);

        protected abstract IEnumerable<MigrationMetadata> InternalGetAllMigrationMetadata();

        protected abstract void CreateTableIfNotExists();
    }
}
