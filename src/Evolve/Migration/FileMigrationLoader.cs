using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class FileMigrationLoader : IMigrationLoader
    {
        private const string InvalidMigrationScriptLocation = "Invalid migration script location: {0}.";
        private const string DuplicateMigrationScriptVersion = "Found multiple sql migration files with the same version: {0}.";

        public IEnumerable<MigrationScript> GetMigrations(IEnumerable<string> locations, string prefix, string separator, string suffix)
        {
            Check.HasNoNulls(locations, nameof(locations));
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __
            Check.NotNullOrEmpty(suffix, nameof(suffix)); // .sql

            var migrations = new List<MigrationScript>();

            string searchPattern = $"{prefix}*{suffix}"; // "V*.sql"

            foreach (string location in locations.Distinct(StringComparer.OrdinalIgnoreCase)) // Remove duplicate locations if any
            {
                DirectoryInfo dirToScan = ResolveDirectory(location);
                if(!dirToScan.Exists) continue;

                dirToScan.GetFiles(searchPattern, SearchOption.AllDirectories)   // Get scripts recursively
                         .Where(f => !migrations.Any(m => m.Path == f.FullName)) // Scripts not already loaded
                         .ToList()
                         .ForEach(f => migrations.Add(LoadMigrationFromFile(f.FullName, prefix, separator)));
            }

            var duplicates = migrations.GroupBy(x => x.Version)
                                       .Where(grp => grp.Count() > 1)
                                       .Select(grp => grp.Key.Label)
                                       .ToArray();

            if(duplicates.Count() > 0)
            {
                throw new EvolveConfigurationException(string.Format(DuplicateMigrationScriptVersion, string.Join(", ", duplicates)));
            }

            return migrations.OrderBy(x => x.Version).ToList();
        }

        private MigrationScript LoadMigrationFromFile(string script, string prefix, string separator)
        {
            Check.FileExists(script, nameof(script)); // V1_3_1__Migration_description.sql
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __

            MigrationUtil.ExtractVersionAndDescription(script, prefix, separator, out string version, out string description);
            return new MigrationScript(script, version, description);
        }

        private DirectoryInfo ResolveDirectory(string location)
        {
            Check.NotNullOrEmpty(location, nameof(location));

            try
            {
                return new DirectoryInfo(location);
            }
            catch (Exception ex)
            {
                throw new EvolveConfigurationException(string.Format(InvalidMigrationScriptLocation, location), ex);
            }
        }
    }
}
