using Evolve.Utilities;
using System;
using System.Collections.Generic;

namespace Evolve.Migration
{
    public class FileMigrationLoader : IMigrationLoader
    {
        public IEnumerable<MigrationScript> GetMigrations(IEnumerable<string> locations)
        {
            Check.HasNoNulls(locations, nameof(locations));

            throw new NotImplementedException();
        }
    }
}
