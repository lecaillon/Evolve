using System.Collections.Generic;

namespace Evolve.Migration
{
    public interface IMigrationLoader
    {
        IEnumerable<MigrationScript> GetMigrations(IEnumerable<string> locations, string prefix, string separator, string suffix);
    }
}
