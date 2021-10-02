using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Evolve.Configuration;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    /// <summary>
    ///     A migration loader that searchs migrations embedded in assemblies. 
    /// </summary>
    public class EmbeddedResourceMigrationLoader : IMigrationLoader
    {
        private const string InvalidEmbeddedResourceFormat = "Embedded resource {0} has an invalid format.";
        protected readonly IEvolveConfiguration _options;

        /// <summary>
        ///     Initialize a new instance of the <see cref="FileMigrationLoader"/> class.
        /// </summary>
        /// <param name="options"> Evolve configuration </param>
        public EmbeddedResourceMigrationLoader(in IEvolveConfiguration options)
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
            var migrations = new List<EmbeddedResourceMigrationScript>();
            var encoding = _options.Encoding ?? Encoding.UTF8;
            string prefix = Check.NotNullOrEmpty(_options.SqlMigrationPrefix, nameof(_options.SqlMigrationPrefix));
            string suffix = Check.NotNullOrEmpty(_options.SqlMigrationSuffix, nameof(_options.SqlMigrationSuffix));
            string separator = Check.NotNullOrEmpty(_options.SqlMigrationSeparator, nameof(_options.SqlMigrationSeparator));
            var assemblies = _options.EmbeddedResourceAssemblies is null ? new List<Assembly>() : Check.HasNoNulls(_options.EmbeddedResourceAssemblies, nameof(_options.EmbeddedResourceAssemblies));
            var filters = _options.EmbeddedResourceFilters is null ? new List<string>() : Check.HasNoNulls(_options.EmbeddedResourceFilters, nameof(_options.EmbeddedResourceFilters));

            foreach (var assembly in assemblies)
            {
                assembly.GetManifestResourceNames()
                        .Where(x => !filters.Any() || filters.Any(f => x.StartsWith(f, StringComparison.OrdinalIgnoreCase)))
                        .Where(x => x.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        .Where(x => GetFileName(x).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        .Select(x =>
                        {
                            MigrationUtil.ExtractVersionAndDescription(GetFileName(x), prefix, separator, out string version, out string description);
                            return new EmbeddedResourceMigrationScript(
                                version,
                                description,
                                name: GetFileName(x),
                                content: assembly.GetManifestResourceStream(x)!,
                                type: MetadataType.Migration,
                                encoding);
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
            var migrations = new List<EmbeddedResourceMigrationScript>();
            var encoding = _options.Encoding ?? Encoding.UTF8;
            string prefix = Check.NotNullOrEmpty(_options.SqlRepeatableMigrationPrefix, nameof(_options.SqlRepeatableMigrationPrefix));
            string suffix = Check.NotNullOrEmpty(_options.SqlMigrationSuffix, nameof(_options.SqlMigrationSuffix));
            string separator = Check.NotNullOrEmpty(_options.SqlMigrationSeparator, nameof(_options.SqlMigrationSeparator));
            var assemblies = _options.EmbeddedResourceAssemblies is null ? new List<Assembly>() : Check.HasNoNulls(_options.EmbeddedResourceAssemblies, nameof(_options.EmbeddedResourceAssemblies));
            var filters = _options.EmbeddedResourceFilters is null ? new List<string>() : Check.HasNoNulls(_options.EmbeddedResourceFilters, nameof(_options.EmbeddedResourceFilters));

            foreach (var assembly in assemblies)
            {
                assembly.GetManifestResourceNames()
                        .Where(x => !filters.Any() || filters.Any(f => x.StartsWith(f, StringComparison.OrdinalIgnoreCase)))
                        .Where(x => x.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        .Where(x => GetFileName(x).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        .Select(x =>
                        {
                            MigrationUtil.ExtractDescription(GetFileName(x), prefix, separator, out string description);
                            return new EmbeddedResourceMigrationScript(
                                version: null,
                                description,
                                name: GetFileName(x),
                                content: assembly.GetManifestResourceStream(x)!,
                                type: MetadataType.RepeatableMigration,
                                encoding);
                        })
                        .ToList()
                        .ForEach(x => migrations.Add(x));
            }

            return migrations.CheckForDuplicateName()
                             .OrderBy(x => x.Name)
                             .Cast<MigrationScript>()
                             .ToList();
        }

        private static string GetFileName(string resource)
        {
            Check.NotNullOrEmpty(resource, nameof(resource));

            string[] parts = resource.Split('.');
            if (parts.Length < 2)
            {
                throw new EvolveConfigurationException(string.Format(InvalidEmbeddedResourceFormat, resource));
            }
            return parts[parts.Length - 2] + "." + parts.Last();
        }
    }
}
