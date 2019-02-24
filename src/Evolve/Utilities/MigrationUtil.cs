﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evolve.Migration;

namespace Evolve.Utilities
{
    public static class MigrationUtil
    {
        private const string MigrationNamePrefixNotFound = "Prefix {0} not found in sql file name: {1}.";
        private const string MigrationNameSeparatorNotFound = "Separator {0} not found in sql file name: {1}.";
        private const string MigrationNameVersionNotFound = "No version found in sql file name: {0}.";
        private const string MigrationNameDescriptionNotFound = "No description found in sql file name: {0}.";
        private const string DuplicateMigrationScriptVersion = "Found multiple sql migration files with the same version: {0}.";

        public static void ExtractVersionAndDescription(string script, string prefix, string separator, out string version, out string description)
        {
            Check.NotNullOrEmpty(script, nameof(script)); // V1_3_1__Migration_description.sql
            Check.NotNullOrEmpty(prefix, nameof(prefix)); // V
            Check.NotNullOrEmpty(separator, nameof(separator)); // __

            // Check prefix
            //Resolved issuer with .net 3.5
            if (!Path.GetFileNameWithoutExtension(script).Substring(0, prefix.Length).Equals(prefix))
                throw new EvolveConfigurationException(string.Format(MigrationNamePrefixNotFound, prefix, script));

            string migrationName = Path.GetFileNameWithoutExtension(script).Substring(prefix.Length); // 1_3_1__Migration_description

            // Check separator
            int indexOfSeparator = migrationName.IndexOf(separator);
            if (indexOfSeparator == -1)
                throw new EvolveConfigurationException(string.Format(MigrationNameSeparatorNotFound, separator, script));

            version = new string(migrationName.Take(indexOfSeparator).ToArray());
            description = migrationName.Substring(indexOfSeparator + separator.Length)
                                       .Replace("_", " ");

            // Check version
            if (version.IsNullOrWhiteSpace())
                throw new EvolveConfigurationException(string.Format(MigrationNameVersionNotFound, script));

            // Check description
            if (description.IsNullOrWhiteSpace())
                throw new EvolveConfigurationException(string.Format(MigrationNameDescriptionNotFound, script));
        }

        public static IEnumerable<MigrationBase> CheckForDuplicates(this IEnumerable<MigrationBase> migrations)
        {
            Check.NotNull(migrations, nameof(migrations));

            var duplicates = migrations.GroupBy(x => x.Version)
                                       .Where(grp => grp.Count() > 1)
                                       .Select(grp => grp.Key.Label)
                                       .ToArray();

            if (duplicates.Count() > 0)
            {
                throw new EvolveConfigurationException(string.Format(DuplicateMigrationScriptVersion, string.Join(", ", duplicates)));
            }

            return migrations;
        }
    }
}
