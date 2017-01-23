using System;
using System.Collections.Generic;

namespace Evolve.Migration
{
    public class FileMigrationLoader : IMigrationLoader
    {
        public IEnumerable<MigrationScript> GetMigrations(IEnumerable<string> Locations)
        {
            throw new NotImplementedException();
        }
    }
}
