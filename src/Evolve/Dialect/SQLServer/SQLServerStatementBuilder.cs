using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Evolve.Dialect.SQLServer
{
    public class SQLServerStatementBuilder : SqlStatementBuilderBase
    {
        public override string BatchDelimiter => "GO";

        protected override IEnumerable<SqlStatement> Parse(string sqlScript)
        {
            return SplitSqlScript(sqlScript)
                .Select(x => new SqlStatement(sql: x, lineNumber: 0, mustExecuteInTransaction: true));
        }

        /// <summary>
        ///     Splits the <paramref name="sqlScript"/> based on the <see cref="BatchDelimiter"/> pattern.
        /// </summary>
        /// <param name="sqlScript"> The script to parse. </param>
        /// <returns> A list of sql statement. </returns>
        private IEnumerable<string> SplitSqlScript(string sqlScript)
        {
            if (sqlScript.IsNullOrWhiteSpace()) return new List<string>();

            // Split by delimiter
            var statements = Regex.Split(sqlScript, $@"^[\t ]*{BatchDelimiter}(?!\w)[\t ]*\d*[\t ]*(?:--.*)?", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            // Remove empties, trim, and return
            return statements.Where(x => !x.IsNullOrWhiteSpace())
                             .Select(x => x.Trim(' ', '\r', '\n'));
        }
    }
}
