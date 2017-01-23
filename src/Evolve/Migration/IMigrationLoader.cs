using System.Collections.Generic;

namespace Evolve.Migration
{
    public interface IMigrationLoader
    {
        IEnumerable<MigrationScript> GetMigrations(IEnumerable<string> Locations);
    }
}
