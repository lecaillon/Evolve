using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EvolveDb.Configuration;
using EvolveDb.Metadata;
using EvolveDb.Utilities;

namespace EvolveDb.Migration
{
    /// <summary>
    ///     A migration loader that searchs recursively in <see cref="IEvolveConfiguration.Locations"/> 
    ///     for migration files with a specific file name structure.
    /// </summary>
    public class FileMigrationLoader : IMigrationLoader
    {
        private const string InvalidMigrationScriptLocation = "Invalid migration script location: {0}.";
        protected readonly IEvolveConfiguration _options;

        /// <summary>
        ///     Initialize a new instance of the <see cref="FileMigrationLoader"/> class.
        /// </summary>
        /// <param name="options"> Evolve configuration </param>
        public FileMigrationLoader(in IEvolveConfiguration options)
        {
            _options = Check.NotNull(options, nameof(options));
        }

        /// <summary>
        ///     Returns a list of migration scripts ordered by version.
        /// </summary>
        /// <returns> A list of migration script. </returns>
        /// <exception cref="EvolveException"> Throws EvolveException when duplicate version found. </exception>
        public virtual IEnumerable<MigrationScript> GetMigrations()
        {
            var migrations = new List<FileMigrationScript>();
            var encoding = _options.Encoding ?? Encoding.UTF8;
            string prefix = Check.NotNullOrEmpty(_options.SqlMigrationPrefix, nameof(_options.SqlMigrationPrefix));
            string suffix = Check.NotNullOrEmpty(_options.SqlMigrationSuffix, nameof(_options.SqlMigrationSuffix));
            string separator = Check.NotNullOrEmpty(_options.SqlMigrationSeparator, nameof(_options.SqlMigrationSeparator));
            var locations = _options.Locations is null ? new List<string>() : Check.HasNoNulls(_options.Locations, nameof(_options.Locations));

            foreach (string location in locations.Distinct(StringComparer.OrdinalIgnoreCase)) // Remove duplicate locations if any
            {
                DirectoryInfo dirToScan = ResolveDirectory(location);
                if (!dirToScan.Exists)
                {
                    continue;
                }

                GetNotHiddenFilesRecursive(dirToScan, "*")
                         .Where(f => !migrations.Any(m => m.Path == f.FullName) // Scripts not already loaded
                                  && f.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) // "V*"
                                  && f.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) // "*.sql"
                         .Select(f =>
                         {
                             MigrationUtil.ExtractVersionAndDescription(f.FullName, prefix, separator, out string version, out string description);
                             return new FileMigrationScript(path: f.FullName, version, description, MetadataType.Migration, encoding);
                         })
                         .ToList()
                         .ForEach(x => migrations.Add(x));
            }

            return migrations.CheckForDuplicateVersion()
                             .OrderBy(x => x.Version)
                             .Cast<MigrationScript>()
                             .ToList();
        }

        /// <summary>
        ///     Returns a list of repeatable migration scripts ordered by name.
        /// </summary>
        /// <returns> A list of repeatable migration script. </returns>
        /// <exception cref="EvolveException"> Throws EvolveException when duplicate name found. </exception>
        public virtual IEnumerable<MigrationScript> GetRepeatableMigrations()
        {
            var migrations = new List<FileMigrationScript>();
            var encoding = _options.Encoding ?? Encoding.UTF8;
            string prefix = Check.NotNullOrEmpty(_options.SqlRepeatableMigrationPrefix, nameof(_options.SqlRepeatableMigrationPrefix));
            string suffix = Check.NotNullOrEmpty(_options.SqlMigrationSuffix, nameof(_options.SqlMigrationSuffix));
            string separator = Check.NotNullOrEmpty(_options.SqlMigrationSeparator, nameof(_options.SqlMigrationSeparator));
            var locations = _options.Locations is null ? new List<string>() : Check.HasNoNulls(_options.Locations, nameof(_options.Locations));

            foreach (string location in locations.Distinct(StringComparer.OrdinalIgnoreCase)) // Remove duplicate locations if any
            {
                DirectoryInfo dirToScan = ResolveDirectory(location);
                if (!dirToScan.Exists)
                {
                    continue;
                }

                GetNotHiddenFilesRecursive(dirToScan, "*")
                         .Where(f => !migrations.Any(m => m.Path == f.FullName) // Scripts not already loaded
                             && f.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) // "R*"
                             && f.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) // "*.sql"
                         .Select(f =>
                         {
                             MigrationUtil.ExtractDescription(f.FullName, prefix, separator, out string description);
                             return new FileMigrationScript(f.FullName, version: null, description, MetadataType.RepeatableMigration, encoding);
                         })
                         .ToList()
                         .ForEach(x => migrations.Add(x));
            }

            return migrations.CheckForDuplicateName()
                             .OrderBy(x => x.Name)
                             .Cast<MigrationScript>()
                             .ToList();
        }

        private static DirectoryInfo ResolveDirectory(string location)
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

        private IEnumerable<FileInfo> GetNotHiddenFilesRecursive(DirectoryInfo dir, string pattern)
        {
            var files = dir.GetFiles(pattern).Where(f => NotHiddenOrSystem(f.Attributes));

            foreach (var file in files)
            {
                yield return file;
            }

            var nestedDirectories = dir.GetDirectories().Where(d => NotHiddenOrSystem(d.Attributes));

            var nestedFiles = nestedDirectories.SelectMany(d => GetNotHiddenFilesRecursive(d, pattern));

            foreach (var file in nestedFiles)
            {
                yield return file;
            }

            static bool NotHiddenOrSystem(FileAttributes attributes)
            {
                return !attributes.HasFlag(FileAttributes.Hidden)
                && !attributes.HasFlag(FileAttributes.System);
            }
        }
    }
}
