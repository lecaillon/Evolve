using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Evolve.Utilities
{
    public static class MigrationUtil
    {
        private const string MigrationNamePrefixNotFound = "Prefix {0} not found in sql file name: {1}.";
        private const string MigrationNameSeparatorNotFound = "Separator {0} not found in sql file name: {1}.";
        private const string MigrationNameVersionNotFound = "No version found in sql file name: {0}.";
        private const string MigrationNameDescriptionNotFound = "No description found in sql file name: {0}.";

        public static void ExtractVersionAndDescription(string script, string prefix, string separator, out string version, out string description)
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

        public static IEnumerable<string> SplitSqlStatements(string sql, string delimiter)
        {
            if (sql.IsNullOrWhiteSpace()) return new List<string>();
            if (delimiter.IsNullOrWhiteSpace()) return new List<string> { sql };

            // Split by delimiter
            var statements = Regex.Split(sql, $@"^[\t ]*{delimiter}(?!\w)[\t ]*\d*[\t ]*(?:--.*)?", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            // Remove empties, trim, and return
            return statements.Where(x => !x.IsNullOrWhiteSpace())
                             .Select(x => x.Trim(' ', '\r', '\n'));
        }
    }
}
