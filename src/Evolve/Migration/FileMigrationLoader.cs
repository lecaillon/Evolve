using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class FileMigrationLoader : IMigrationLoader
    {
        private const string InvalidMigrationScriptLocation = "Invalid migration script location: {0}.";
        private readonly IEnumerable<string> _locations;

        /// <summary>
        ///     Initialize a new instance of the <see cref="FileMigrationLoader"/> class.
        /// </summary>
        /// <param name="locations"> List of paths to scan recursively for migrations. </param>
        public FileMigrationLoader(IEnumerable<string> locations)
        {
            _locations = locations is null ? new List<string>() : Check.HasNoNulls(locations, nameof(locations));
        }

        public IEnumerable<MigrationScript> GetMigrations(string prefix, string separator, string suffix, Encoding encoding = null)
        {
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __
            Check.NotNullOrEmpty(suffix, nameof(suffix)); // .sql

            var migrations = new List<FileMigrationScript>();

            string searchPattern = $"{prefix}*{suffix}"; // "V*.sql"

            foreach (string location in _locations.Distinct(StringComparer.OrdinalIgnoreCase)) // Remove duplicate locations if any
            {
                DirectoryInfo dirToScan = ResolveDirectory(location);
                if(!dirToScan.Exists) continue;

                dirToScan.GetFiles(searchPattern, SearchOption.AllDirectories)   // Get scripts recursively
                         .Where(f => !migrations.Any(m => m.Path == f.FullName)) // Scripts not already loaded
                         .ToList()
                         .ForEach(f => migrations.Add(LoadMigrationFromFile(f.FullName, prefix, separator, encoding ?? Encoding.UTF8)));
            }

            return migrations.Cast<MigrationBase>() // NET 3.5
                             .CheckForDuplicates()
                             .OrderBy(x => x.Version)
                             .Cast<MigrationScript>() // NET 3.5
                             .ToList();
        }

        private FileMigrationScript LoadMigrationFromFile(string script, string prefix, string separator, Encoding textEncoding)
        {
            Check.FileExists(script, nameof(script)); // V1_3_1__Migration_description.sql
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __
            Check.NotNull(textEncoding, nameof(textEncoding));

            MigrationUtil.ExtractVersionAndDescription(script, prefix, separator, out string version, out string description);
            return new FileMigrationScript(script, version, description, MetadataType.Migration, textEncoding);
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
