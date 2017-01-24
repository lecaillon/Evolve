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
        private const string MigrationNameSeparatorNotFound = "Separator {0} not found in migration name: {1}.";

        public IEnumerable<MigrationScript> GetMigrations(IEnumerable<string> locations, string prefix, string separator, string suffix)
        {
            Check.HasNoNulls(locations, nameof(locations));
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __
            Check.NotNullOrEmpty(suffix, nameof(suffix)); // .sql

            var migrations = new List<MigrationScript>();

            string searchPattern = $"{prefix}*{suffix}"; // "V*.sql"
            foreach (string location in locations)
            {
                DirectoryInfo dirToScan = ResolveDirectory(location);
                dirToScan.GetFiles(searchPattern, SearchOption.AllDirectories)   // Get scripts recursively
                         .Where(f => !migrations.Any(m => m.Path == f.FullName)) // Scripts not already loaded
                         .ToList()
                         .ForEach(f => migrations.Add(LoadMigrationFromFile(f.FullName, prefix, separator)));
            }

            return migrations;
        }

        private Tuple<string, string> ExtractVersionAndDescription(string script, string prefix, string separator)
        {
            Check.FileExists(script, nameof(script)); // V1_3_1__Migration-description.sql
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __

            string migrationName = Path.GetFileNameWithoutExtension(script).Substring(prefix.Length); // 1_3_1__Migration-description
            int indexOfSeparator = migrationName.IndexOf(separator);
            if (indexOfSeparator == -1)
                throw new EvolveConfigurationException(string.Format(MigrationNameSeparatorNotFound, separator, migrationName));

            string version = new string(migrationName.Take(indexOfSeparator).ToArray());
            string description = migrationName.Substring(indexOfSeparator + separator.Length)
                                              .Replace(separator, " ");

            return Tuple.Create(version, description);
        }

        private MigrationScript LoadMigrationFromFile(string script, string prefix, string separator)
        {
            Check.FileExists(script, nameof(script)); // V1_3_1__Migration-description.sql
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __

            var versionAndDescription = ExtractVersionAndDescription(script, prefix, separator);
            return new MigrationScript(script, versionAndDescription.Item1, versionAndDescription.Item2);
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
