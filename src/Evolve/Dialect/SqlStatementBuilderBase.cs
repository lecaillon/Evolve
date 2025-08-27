using EvolveDb.Configuration;
using EvolveDb.Migration;
using EvolveDb.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace EvolveDb.Dialect
{
    /// <summary>
    ///     A base class used to parse a SQL script and return a list of sql statements.
    ///     Each statement can then be executed depending on its own database constraints,enlisted or not in a transaction.
    /// </summary>
    internal abstract class SqlStatementBuilderBase
    {
        /// <summary>
        ///     Gets the database batch delimiter.
        /// </summary>
        public abstract string? BatchDelimiter { get; }

        /// <summary>
        ///     Returns a <see cref="List{SqlStatement}"/> given a <paramref name="migrationScript"/>.
        /// </summary>
        /// <remarks>
        ///     Placeholders are replaced by their values in the migration script.
        ///     The result is then parsed in sql statements: <see cref="Parse(string, bool)"/>.
        /// </remarks>
        /// <param name="migrationScript"> The sql script to parse. </param>
        /// <param name="placeholders"> The placeholders to replace. </param>
        /// <returns> A <see cref="List{SqlStatement}"/> to execute individually in a command. </returns>
        public virtual IEnumerable<SqlStatement> LoadSqlStatements(MigrationScript migrationScript, Dictionary<string, string> placeholders)
        {
            return LoadSqlStatements(migrationScript, placeholders, null, null);
        }

        /// <summary>
        ///     Returns a <see cref="List{SqlStatement}"/> given a <paramref name="migrationScript"/> with optional SQL linting.
        /// </summary>
        /// <remarks>
        ///     Placeholders are replaced by their values in the migration script.
        ///     The result is then parsed in sql statements: <see cref="Parse(string, bool)"/>.
        ///     If linting is enabled, statements are analyzed for unsafe DDL patterns.
        /// </remarks>
        /// <param name="migrationScript"> The sql script to parse. </param>
        /// <param name="placeholders"> The placeholders to replace. </param>
        /// <param name="enableSqlLint"> Whether to enable SQL linting. </param>
        /// <param name="sqlLintFailureLevel"> How to handle lint failures. </param>
        /// <param name="logAction"> Optional logging action for lint warnings. </param>
        /// <returns> A <see cref="List{SqlStatement}"/> to execute individually in a command. </returns>
        public virtual IEnumerable<SqlStatement> LoadSqlStatements(MigrationScript migrationScript, Dictionary<string, string> placeholders, bool? enableSqlLint, SqlLintFailureLevel? sqlLintFailureLevel, System.Action<string>? logAction = null)
        {
            Check.NotNull(migrationScript, nameof(migrationScript));
            Check.NotNull(placeholders, nameof(placeholders));
            string sql = migrationScript.Content; // copy the content of the migration script
            foreach (var entry in placeholders)
            {
                sql = sql.Replace(entry.Key, entry.Value);
            }

            var statements = Parse(sql, migrationScript.IsTransactionEnabled).ToList();

            // Perform SQL linting if enabled
            if (enableSqlLint == true)
            {
                var lintIssues = SqlLinter.AnalyzeStatements(statements).ToList();
                if (lintIssues.Any())
                {
                    var failureLevel = sqlLintFailureLevel ?? SqlLintFailureLevel.Warning;

                    if (failureLevel == SqlLintFailureLevel.Error)
                    {
                        throw new EvolveSqlLintException(lintIssues);
                    }
                    else if (logAction != null)
                    {
                        // Log warnings
                        foreach (var issue in lintIssues)
                        {
                            var lineInfo = issue.LineNumber > 0 ? $" (line {issue.LineNumber})" : "";
                            logAction($"SQL Lint Warning in {migrationScript.Name}{lineInfo}: {issue.Message}");
                        }
                    }
                }
            }

            return statements;
        }

        /// <summary>
        ///     Parse a SQL script into a list of SQL statement to execute.
        /// </summary>
        protected abstract IEnumerable<SqlStatement> Parse(string sqlScript, bool transactionEnabled);
    }
}
