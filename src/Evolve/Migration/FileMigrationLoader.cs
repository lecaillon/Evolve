using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    internal class FileMigrationLoader : IMigrationLoader
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

        public IEnumerable<MigrationScript> GetMigrations(string prefix, string separator, string suffix, Encoding? encoding = null)
        {
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __
            Check.NotNullOrEmpty(suffix, nameof(suffix)); // .sql

            var migrations = new List<FileMigrationScript>();
            encoding ??= Encoding.UTF8;

            foreach (string location in _locations.Distinct(StringComparer.OrdinalIgnoreCase)) // Remove duplicate locations if any
            {
                DirectoryInfo dirToScan = ResolveDirectory(location);
                if(!dirToScan.Exists) continue;

                dirToScan.GetFiles("*", SearchOption.AllDirectories) // Get scripts recursively
                         .Where(f => !migrations.Any(m => m.Path == f.FullName) // Scripts not already loaded
                                  //&& f.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) // "V*"
                                  && f.Name.Substring(f.Name.IndexOf(separator) + separator.Length, prefix.Length).Equals(prefix, StringComparison.OrdinalIgnoreCase) // "*V*"
                                  && f.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) // "*.sql"
                         .Select(f =>
                         {
                             MigrationUtil.ExtractVersionAndDescription(f.FullName, prefix, separator, out string version, out string description);
                             return new FileMigrationScript(path: f.FullName, version, description, MetadataType.Migration, encoding);
                         })
                         .ToList()
                         .ForEach(x => migrations.Add(x));
            }

            return migrations.Cast<MigrationBase>() // NET 3.5
                             .CheckForDuplicateVersion()
                             .OrderBy(x => x.Version)
                             .Cast<MigrationScript>() // NET 3.5
                             .ToList();
        }

        public IEnumerable<MigrationScript> GetRepeatableMigrations(string prefix, string separator, string suffix, Encoding? encoding = null)
        {
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // R
            Check.NotNullOrEmpty(separator, nameof(separator)); // __
            Check.NotNullOrEmpty(suffix, nameof(suffix)); // .sql

            var migrations = new List<FileMigrationScript>();
            encoding ??= Encoding.UTF8;

            foreach (string location in _locations.Distinct(StringComparer.OrdinalIgnoreCase)) // Remove duplicate locations if any
            {
                DirectoryInfo dirToScan = ResolveDirectory(location);
                if (!dirToScan.Exists) continue;

                dirToScan.GetFiles("*", SearchOption.AllDirectories)   // Get scripts recursively
                         .Where(f => !migrations.Any(m => m.Path == f.FullName) // Scripts not already loaded
                             //&& f.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) // "R*"
                                  && f.Name.Substring(f.Name.IndexOf(separator) + separator.Length, prefix.Length).Equals(prefix, StringComparison.OrdinalIgnoreCase) // "*R*"
                             && f.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) // "*.sql"
                         .Select(f =>
                         {
                             MigrationUtil.ExtractDescription(f.FullName, prefix, separator, out string description);
                             return new FileMigrationScript(f.FullName, version: null, description, MetadataType.RepeatableMigration, encoding);
                         })
                         .ToList()
                         .ForEach(x => migrations.Add(x));
            }

            return migrations.Cast<MigrationBase>() // NET 3.5
                             .CheckForDuplicateName()
                             .OrderBy(x => x.Name)
                             .Cast<MigrationScript>() // NET 3.5
                             .ToList();
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
