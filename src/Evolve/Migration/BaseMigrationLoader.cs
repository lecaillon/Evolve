using System.Collections.Generic;
using System.Linq;

namespace Evolve.Migration
{
    public abstract class BaseMigrationLoader: IMigrationLoader
    {
        protected const string DuplicateMigrationScriptVersion = "Found multiple sql migration files with the same version: {0}.";

        public abstract IEnumerable<IMigrationScript> GetMigrations(IEnumerable<string> locations, string prefix, string separator, string suffix);
        protected static void CheckForDuplicates(IEnumerable<IMigrationScript> migrations)
        {
            var duplicates = migrations.GroupBy(x => x.Version)
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.Key.Label)
                .ToArray();

            if (duplicates.Any())
            {
                throw new EvolveConfigurationException(string.Format(DuplicateMigrationScriptVersion,
                    string.Join(", ", duplicates)));
            }
        }
    }
}