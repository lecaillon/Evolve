using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class EmbeddedResourceMigrationLoader: BaseMigrationLoader
    {
        private readonly Assembly _resourceContext;

        public EmbeddedResourceMigrationLoader(Assembly resourceContext)
        {
            _resourceContext = resourceContext;
        }

        private static string ResourceRefToName(string resourceRef)
        {
            string[] parts = resourceRef.Split('.');
            return parts[parts.Length - 2] + "." + parts.Last();
        }

        public override IEnumerable<IMigrationScript> GetMigrations(IEnumerable<string> locations, string prefix, string separator, string suffix)
        {
            List<string> locationList = locations.ToList();

            Check.HasNoNulls(locationList, nameof(locations));
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __
            Check.NotNullOrEmpty(suffix, nameof(suffix)); // .sql

            string[] resourceRefs = _resourceContext.GetManifestResourceNames();
            var migrations = resourceRefs.AsEnumerable()
                .Where(resourceRef => resourceRef.EndsWith(suffix, StringComparison.CurrentCultureIgnoreCase))
                .Where(resourceRef => locationList.Any(resourceRef.StartsWith))
                .Select(resourceRef =>
                {
                    var name = ResourceRefToName(resourceRef);
                    MigrationUtil.ExtractVersionAndDescription(name, prefix, separator, out string version,
                        out string description);
                    return (IMigrationScript) new EmbeddedResourceMigrationScript(version, name, description,
                        () => new StreamReader(
                            _resourceContext.GetManifestResourceStream(resourceRef) ??
                            throw new InvalidOperationException($"Bad resource reference: {resourceRef}"),
                            Encoding.UTF8));
                })
                .OrderBy(migration => migration.Version).ToList();

            CheckForDuplicates(migrations);

            return migrations;
        }
    }
}