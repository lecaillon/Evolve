using Evolve.Migration;
using System.Collections.Generic;

namespace Evolve.Metadata
{
    public interface IEvolveMetadata
    {
        EndedMigration AddEndedMigration(MigrationScript migration);

        IEnumerable<EndedMigration> GetAllMigrations();
    }
}
