using Evolve.Migration;
using System.Collections.Generic;

namespace Evolve.Metadata
{
    public interface IEvolveMetadata
    {
        MigrationMetadata AddMigrationMetadata(MigrationScript migration);

        IEnumerable<MigrationMetadata> GetAllMigrationMetadata();
    }
}
