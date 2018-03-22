using System.Collections.Generic;
using Evolve.Migration;
using Evolve.Utilities;

namespace Evolve.Dialect
{
    public abstract class SqlStatementBuilder
    {
        public virtual IEnumerable<SqlStatement> LoadSqlStatements(MigrationScript migrationScript, 
                                                                   Dictionary<string, string> placeholders, 
                                                                   string delimiter)
        {
            Check.NotNull(migrationScript, nameof(migrationScript));
            Check.NotNull(placeholders, nameof(placeholders));

            string sqlScript = migrationScript.Content; // copy the content of the migration script
            foreach (var entry in placeholders)
            {
                sqlScript = sqlScript.Replace(entry.Key, entry.Value);
            }

            return Parse(sqlScript, delimiter);
        }

        protected abstract IEnumerable<SqlStatement> Parse(string sqlScript, string delimiter);
    }
}
