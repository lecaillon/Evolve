using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EvolveDb.Dialect
{
    /// <summary>
    ///     Represents a SQL lint issue found during analysis.
    /// </summary>
    public class SqlLintIssue
    {
        public SqlLintIssue(string message, int lineNumber, string statement)
        {
            Message = message;
            LineNumber = lineNumber;
            Statement = statement;
        }

        /// <summary>
        ///     Gets the description of the lint issue.
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///     Gets the line number where the issue was found.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        ///     Gets the SQL statement that contains the issue.
        /// </summary>
        public string Statement { get; }
    }

    /// <summary>
    ///     Analyzes SQL statements for potentially unsafe DDL patterns.
    /// </summary>
    internal class SqlLinter
    {
        private static readonly Regex DropTablePattern = new Regex(
            @"^\s*DROP\s+TABLE\s+(?!IF\s+EXISTS\s+)\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex CreateTablePattern = new Regex(
            @"^\s*CREATE\s+TABLE\s+(?!IF\s+NOT\s+EXISTS\s+)\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex DropSchemaPattern = new Regex(
            @"^\s*DROP\s+(?:SCHEMA|DATABASE)\s+(?!IF\s+EXISTS\s+)\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex CreateSchemaPattern = new Regex(
            @"^\s*CREATE\s+(?:SCHEMA|DATABASE)\s+(?!IF\s+NOT\s+EXISTS\s+)\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex DropViewPattern = new Regex(
            @"^\s*DROP\s+VIEW\s+(?!IF\s+EXISTS\s+)\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex CreateViewPattern = new Regex(
            @"^\s*CREATE\s+VIEW\s+(?!IF\s+NOT\s+EXISTS\s+)\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex DropSequencePattern = new Regex(
            @"^\s*DROP\s+SEQUENCE\s+(?!IF\s+EXISTS\s+)\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex CreateSequencePattern = new Regex(
            @"^\s*CREATE\s+SEQUENCE\s+(?!IF\s+NOT\s+EXISTS\s+)\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        ///     Analyzes SQL statements for unsafe DDL patterns.
        /// </summary>
        /// <param name="statements">The SQL statements to analyze.</param>
        /// <returns>A list of lint issues found.</returns>
        public static IEnumerable<SqlLintIssue> AnalyzeStatements(IEnumerable<SqlStatement> statements)
        {
            var issues = new List<SqlLintIssue>();

            foreach (var statement in statements)
            {
                issues.AddRange(AnalyzeStatement(statement));
            }

            return issues;
        }

        /// <summary>
        ///     Analyzes a single SQL statement for unsafe DDL patterns.
        /// </summary>
        /// <param name="statement">The SQL statement to analyze.</param>
        /// <returns>A list of lint issues found in the statement.</returns>
        private static IEnumerable<SqlLintIssue> AnalyzeStatement(SqlStatement statement)
        {
            var issues = new List<SqlLintIssue>();
            var sql = statement.Sql?.Trim();

            if (string.IsNullOrWhiteSpace(sql))
            {
                return issues;
            }

            // Clean up the SQL to remove comments and string literals to avoid false positives
            var cleanedSql = CleanSqlForAnalysis(sql);

            // Check for DROP TABLE without IF EXISTS
            if (DropTablePattern.IsMatch(cleanedSql))
            {
                issues.Add(new SqlLintIssue(
                    "DROP TABLE statement should use 'IF EXISTS' to avoid errors if the table doesn't exist",
                    statement.LineNumber,
                    sql));
            }

            // Check for CREATE TABLE without IF NOT EXISTS
            if (CreateTablePattern.IsMatch(cleanedSql))
            {
                issues.Add(new SqlLintIssue(
                    "CREATE TABLE statement should use 'IF NOT EXISTS' to avoid errors if the table already exists",
                    statement.LineNumber,
                    sql));
            }

            // Check for DROP SCHEMA/DATABASE without IF EXISTS
            if (DropSchemaPattern.IsMatch(cleanedSql))
            {
                issues.Add(new SqlLintIssue(
                    "DROP SCHEMA/DATABASE statement should use 'IF EXISTS' to avoid errors if it doesn't exist",
                    statement.LineNumber,
                    sql));
            }

            // Check for CREATE SCHEMA/DATABASE without IF NOT EXISTS
            if (CreateSchemaPattern.IsMatch(cleanedSql))
            {
                issues.Add(new SqlLintIssue(
                    "CREATE SCHEMA/DATABASE statement should use 'IF NOT EXISTS' to avoid errors if it already exists",
                    statement.LineNumber,
                    sql));
            }

            // Check for DROP VIEW without IF EXISTS
            if (DropViewPattern.IsMatch(cleanedSql))
            {
                issues.Add(new SqlLintIssue(
                    "DROP VIEW statement should use 'IF EXISTS' to avoid errors if the view doesn't exist",
                    statement.LineNumber,
                    sql));
            }

            // Check for CREATE VIEW without IF NOT EXISTS (note: not all databases support this)
            if (CreateViewPattern.IsMatch(cleanedSql))
            {
                issues.Add(new SqlLintIssue(
                    "CREATE VIEW statement should use 'IF NOT EXISTS' where supported to avoid errors if the view already exists",
                    statement.LineNumber,
                    sql));
            }

            // Check for DROP SEQUENCE without IF EXISTS
            if (DropSequencePattern.IsMatch(cleanedSql))
            {
                issues.Add(new SqlLintIssue(
                    "DROP SEQUENCE statement should use 'IF EXISTS' to avoid errors if the sequence doesn't exist",
                    statement.LineNumber,
                    sql));
            }

            // Check for CREATE SEQUENCE without IF NOT EXISTS
            if (CreateSequencePattern.IsMatch(cleanedSql))
            {
                issues.Add(new SqlLintIssue(
                    "CREATE SEQUENCE statement should use 'IF NOT EXISTS' to avoid errors if the sequence already exists",
                    statement.LineNumber,
                    sql));
            }

            return issues;
        }

        /// <summary>
        ///     Cleans SQL by removing comments and string literals to avoid false positives during analysis.
        /// </summary>
        /// <param name="sql">The SQL to clean.</param>
        /// <returns>The cleaned SQL.</returns>
        private static string CleanSqlForAnalysis(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return string.Empty;

            // Remove single-line comments (-- comments)
            sql = Regex.Replace(sql, @"--.*$", "", RegexOptions.Multiline);

            // Remove multi-line comments (/* comments */)
            sql = Regex.Replace(sql, @"/\*.*?\*/", "", RegexOptions.Singleline);

            // Remove single-quoted string literals
            sql = Regex.Replace(sql, @"'(?:[^'\\]|\\.)*'", " ", RegexOptions.Singleline);

            // Remove double-quoted string literals
            sql = Regex.Replace(sql, @"""(?:[^""\\\\]|\\.)*""", " ", RegexOptions.Singleline);

            return sql;
        }
    }
}