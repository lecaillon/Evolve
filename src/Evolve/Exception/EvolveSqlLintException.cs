using EvolveDb.Dialect;
using System.Collections.Generic;
using System.Linq;

namespace EvolveDb
{
    /// <summary>
    ///     Exception thrown when SQL lint issues are found and configured to fail on errors.
    /// </summary>
    public class EvolveSqlLintException : EvolveException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EvolveSqlLintException"/> class.
        /// </summary>
        /// <param name="issues">The SQL lint issues that caused the exception.</param>
        public EvolveSqlLintException(IEnumerable<SqlLintIssue> issues)
            : base(BuildMessage(issues))
        {
            Issues = issues?.ToList() ?? [];
        }

        /// <summary>
        ///     Gets the SQL lint issues that caused the exception.
        /// </summary>
        public IReadOnlyList<SqlLintIssue> Issues { get; }

        private static string BuildMessage(IEnumerable<SqlLintIssue> issues)
        {
            var issueList = issues?.ToList() ?? [];
            if (!issueList.Any())
            {
                return "SQL lint validation failed with no specific issues.";
            }

            var message = $"SQL lint validation failed with {issueList.Count} issue(s):\n";
            foreach (var issue in issueList)
            {
                var lineInfo = issue.LineNumber > 0 ? $" (line {issue.LineNumber})" : "";
                message += $"- {issue.Message}{lineInfo}\n";
            }

            return message;
        }
    }
}