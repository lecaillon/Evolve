using System.Collections.Generic;
using Evolve.Migration;
using Evolve.Utilities;

namespace Evolve.Dialect
{
    /// <summary>
    ///     A base class used to parse a SQL script and return a list of sql statements.
    ///     Each statement can then be executed with its own constraints, enlisted or not in a transaction.
    /// </summary>
    public abstract class SqlStatementBuilderBase
    {
        /// <summary>
        ///     Gets the database bacth delimiter.
        /// </summary>
        public abstract string BatchDelimiter { get; }

        public virtual IEnumerable<SqlStatement> LoadSqlStatements(MigrationScript migrationScript, Dictionary<string, string> placeholders)
        {
            Check.NotNull(migrationScript, nameof(migrationScript));
            Check.NotNull(placeholders, nameof(placeholders));

            string sqlScript = migrationScript.Content; // copy the content of the migration script
            foreach (var entry in placeholders)
            {
                sqlScript = sqlScript.Replace(entry.Key, entry.Value);
            }

            return Parse(sqlScript);
        }

        protected abstract IEnumerable<SqlStatement> Parse(string sqlScript);
    }
}
