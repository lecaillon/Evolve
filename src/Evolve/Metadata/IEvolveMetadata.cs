using Evolve.Migration;
using System.Collections.Generic;

namespace Evolve.Metadata
{
    public interface IEvolveMetadata
    {
        void Lock();

        bool CreateIfNotExists();

        MigrationMetadata AddMigrationMetadata(MigrationScript migration, bool success);

        IEnumerable<MigrationMetadata> GetAllMigrationMetadata();
    }
}
