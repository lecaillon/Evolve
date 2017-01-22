using System.Collections.Generic;

namespace Evolve.Migration
{
    public interface IMigrationMetadata
    {
        EndedMigration AddEndedMigration(PendingMigration migration);

        IEnumerable<EndedMigration> GetAllMigrations();
    }
}
