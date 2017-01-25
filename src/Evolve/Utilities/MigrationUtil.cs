using System;
using System.IO;
using System.Linq;

namespace Evolve.Utilities
{
    public static class MigrationUtil
    {
        private const string MigrationNamePrefixNotFound = "Prefix {0} not found in migration: {1}.";
        private const string MigrationNameSeparatorNotFound = "Separator {0} not found in migration: {1}.";
        private const string MigrationNameVersionNotFound = "No version found in migration: {0}.";
        private const string MigrationNameDescriptionNotFound = "No description found in migration: {0}.";

        public static Tuple<string, string> ExtractVersionAndDescription(string script, string prefix, string separator)
        {
            Check.NotNullOrEmpty(script, nameof(script)); // V1_3_1__Migration_description.sql
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __

            // Check prefix
            if (string.Concat(Path.GetFileNameWithoutExtension(script).Take(prefix.Length)) != prefix)
                throw new EvolveConfigurationException(string.Format(MigrationNamePrefixNotFound, prefix, script));

            string migrationName = Path.GetFileNameWithoutExtension(script).Substring(prefix.Length); // 1_3_1__Migration_description

            // Check separator
            int indexOfSeparator = migrationName.IndexOf(separator);
            if (indexOfSeparator == -1)
                throw new EvolveConfigurationException(string.Format(MigrationNameSeparatorNotFound, separator, script));

            string version = new string(migrationName.Take(indexOfSeparator).ToArray());
            string description = migrationName.Substring(indexOfSeparator + separator.Length)
                                              .Replace("_", " ");

            // Check version
            if (string.IsNullOrWhiteSpace(version))
                throw new EvolveConfigurationException(string.Format(MigrationNameVersionNotFound, script));

            // Check description
            if (string.IsNullOrWhiteSpace(description))
                throw new EvolveConfigurationException(string.Format(MigrationNameDescriptionNotFound, script));

            return Tuple.Create(version, description);
        }
    }
}
