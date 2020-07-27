using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    internal class EmbeddedResourceMigrationLoader : IMigrationLoader
    {
        private const string InvalidEmbeddedResourceFormat = "Embedded resource {0} has an invalid format.";

        private readonly IEnumerable<Assembly> _assemblies;
        private readonly IEnumerable<string> _filters;

        /// <summary>
        ///     Initialize a new instance of the <see cref="EmbeddedResourceMigrationLoader"/> class.
        /// </summary>
        /// <param name="assemblies"> List of assembly to scan in order to load embedded migration scripts. </param>
        /// <param name="filters"> Filters used to exclude embedded migration scripts that do not start with one of those. </param>
        public EmbeddedResourceMigrationLoader(IEnumerable<Assembly> assemblies, IEnumerable<string> filters)
        {
            _assemblies = assemblies is null ? new List<Assembly>() : Check.HasNoNulls(assemblies, nameof(assemblies));
            _filters = filters is null ? new List<string>() : Check.HasNoNulls(filters, nameof(filters));
        }

        public IEnumerable<MigrationScript> GetMigrations(string prefix, string separator, string suffix, Encoding? encoding = null)
        {
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __
            Check.NotNullOrEmpty(suffix, nameof(suffix)); // .sql

            var migrations = new List<EmbeddedResourceMigrationScript>();
            encoding ??= Encoding.UTF8;

            foreach (var assembly in _assemblies)
            {
                assembly.GetManifestResourceNames()
                        .Where(x => _filters.Any() ? _filters.Any(f => x.StartsWith(f, StringComparison.OrdinalIgnoreCase)) : true)
                        .Where(x => x.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        .Where(x => 
                            //GetFileName(x).StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                            GetFileName(x).Substring(GetFileName(x).IndexOf(separator) + separator.Length, prefix.Length).Equals(prefix, StringComparison.OrdinalIgnoreCase) // "*V*"
                            )
                        .Select(x =>
                        {
                            MigrationUtil.ExtractVersionAndDescription(GetFileName(x), prefix, separator, out string version, out string description);
                            return new EmbeddedResourceMigrationScript(
                                version,
                                description,
                                name: GetFileName(x),
                                content: assembly.GetManifestResourceStream(x),
                                type: MetadataType.Migration,
                                encoding);
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

            var migrations = new List<EmbeddedResourceMigrationScript>();
            encoding ??= Encoding.UTF8;

            foreach (var assembly in _assemblies)
            {
                assembly.GetManifestResourceNames()
                        .Where(x => _filters.Any() ? _filters.Any(f => x.StartsWith(f, StringComparison.OrdinalIgnoreCase)) : true)
                        .Where(x => x.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        .Where(x => 
                            //GetFileName(x).StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                            GetFileName(x).Substring(GetFileName(x).IndexOf(separator) + separator.Length, prefix.Length).Equals(prefix, StringComparison.OrdinalIgnoreCase) // "*R*"
                        )
                        .Select(x =>
                        {
                            MigrationUtil.ExtractDescription(GetFileName(x), prefix, separator, out string description);
                            return new EmbeddedResourceMigrationScript(
                                version: null,
                                description,
                                name: GetFileName(x),
                                content: assembly.GetManifestResourceStream(x),
                                type: MetadataType.RepeatableMigration,
                                encoding);
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

        private string GetFileName(string resource)
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
