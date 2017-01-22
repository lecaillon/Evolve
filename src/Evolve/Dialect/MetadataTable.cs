using Evolve.Metadata;
using Evolve.Migration;
using System.Collections.Generic;

namespace Evolve.Dialect
{
    public abstract class MetadataTable : IEvolveMetadata
    {
        public EndedMigration AddEndedMigration(MigrationScript migration)
        {
            CreateTableIfNotExists();
            return InternalAddEndedMigration(migration);
        }

        public IEnumerable<EndedMigration> GetAllMigrations()
        {
            CreateTableIfNotExists();
            return InternalGetAllMigrations();
        }

        protected abstract EndedMigration InternalAddEndedMigration(MigrationScript migration);

        protected abstract IEnumerable<EndedMigration> InternalGetAllMigrations();

        protected abstract void CreateTableIfNotExists();
    }
}
