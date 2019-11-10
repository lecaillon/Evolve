using System.Collections.Generic;
using Evolve.Migration;
using Evolve.Utilities;

namespace Evolve.Dialect
{
    /// <summary>
    ///     A base class used to parse a SQL script and return a list of sql statements.
    ///     Each statement can then be executed depending its own database constraints,enlisted or not in a transaction.
    /// </summary>
    internal abstract class SqlStatementBuilderBase
    {
        /// <summary>
        ///     Gets the database bacth delimiter.
        /// </summary>
        public abstract string? BatchDelimiter { get; }

        /// <summary>
        ///     Returns a <see cref="List{SqlStatement}"/> given a <paramref name="migrationScript"/>.
        /// </summary>
        /// <remarks>
        ///     Placeholders are replaced by their values in the migration script.
        ///     The result is then parsed in sql statements: <see cref="Parse(string)"/>.
        /// </remarks>
        /// <param name="migrationScript"> The sql script to parse. </param>
        /// <param name="placeholders"> The placeholders to replace. </param>
        /// <returns> A <see cref="List{SqlStatement}"/> to execute individually in a command. </returns>
        public virtual IEnumerable<SqlStatement> LoadSqlStatements(MigrationScript migrationScript, Dictionary<string, string> placeholders)
        {
            Check.NotNull(migrationScript, nameof(migrationScript));
            Check.NotNull(placeholders, nameof(placeholders));

            string sql = migrationScript.Content; // copy the content of the migration script
            foreach (var entry in placeholders)
            {
                sql = sql.Replace(entry.Key, entry.Value);
            }

            return Parse(sql, migrationScript.IsTransactionEnabled());
        }

        protected abstract IEnumerable<SqlStatement> Parse(string sqlScript, bool transactionEnabled);
    }
}
