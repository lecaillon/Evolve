using Evolve.Migration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Dialect
{
    public abstract class MetadataTable : IMigrationMetadata
    {
        public EndedMigration AddEndedMigration(PendingMigration migration)
        {
            CreateTableIfNotExists();
            return InternalAddEndedMigration(migration);
        }

        public IEnumerable<EndedMigration> GetAllMigrations()
        {
            CreateTableIfNotExists();
            return InternalGetAllMigrations();
        }

        protected abstract EndedMigration InternalAddEndedMigration(PendingMigration migration);

        protected abstract IEnumerable<EndedMigration> InternalGetAllMigrations();

        protected abstract void CreateTableIfNotExists();
    }
}
