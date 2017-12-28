using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class FileMigrationLoader : BaseMigrationLoader
    {
        private const string InvalidMigrationScriptLocation = "Invalid migration script location: {0}.";

        public override IEnumerable<IMigrationScript> GetMigrations(IEnumerable<string> locations, string prefix, string separator, string suffix)
        {
            var locationsList = locations.ToList();
            Check.HasNoNulls(locationsList, nameof(locations));
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __
            Check.NotNullOrEmpty(suffix, nameof(suffix)); // .sql

            string searchPattern = $"{prefix}*{suffix}"; // "V*.sql"

            var migrations = locationsList.Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(ResolveDirectory)
                .Where(directory => directory.Exists)
                .SelectMany(directory => directory.GetFiles(searchPattern, SearchOption.AllDirectories))
                .Distinct()
                .Select(file => LoadMigrationFromFile(file.FullName, prefix, separator))
                .OrderBy(migration => migration.Version)
                .ToList();

            CheckForDuplicates(migrations);

            return migrations;
        }



        private IMigrationScript LoadMigrationFromFile(string script, string prefix, string separator)
        {
            Check.FileExists(script, nameof(script)); // V1_3_1__Migration_description.sql
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __

            MigrationUtil.ExtractVersionAndDescription(script, prefix, separator, out string version, out string description);
            return new FileMigrationScript(script, version, description);
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
