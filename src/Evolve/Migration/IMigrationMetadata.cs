using System.Collections.Generic;

namespace Evolve.Migration
{
    public interface IMigrationMetadata
    {
        EndedMigration AddEndedMigration(MigrationScript migration);

        IEnumerable<EndedMigration> GetAllMigrations();
    }
}
