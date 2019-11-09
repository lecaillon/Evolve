using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Evolve.Dialect.SQLServer
{
    /// <summary>
    ///     A SQL Server dedicated builder which handles the statement delimiter GO.
    /// </summary>
    internal class SQLServerStatementBuilder : SqlStatementBuilderBase
    {
        /// <inheritdoc />
        public override string BatchDelimiter => "GO";

        protected override IEnumerable<SqlStatement> Parse(string migrationScript, bool transactionEnabled)
        {
            return ParseBatchDelimiter(migrationScript).Select(sql => new SqlStatement(sql, transactionEnabled));
        }

        private IEnumerable<string> ParseBatchDelimiter(string sqlScript)
        {
            if (sqlScript.IsNullOrWhiteSpace())
            {
                return new List<string>();
            }

            // Split by delimiter
            var statements = Regex.Split(sqlScript, $@"^[\t ]*{BatchDelimiter}(?!\w)[\t ]*\d*[\t ]*(?:--.*)?", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            // Remove empties, trim, and return
            return statements.Where(x => !x.IsNullOrWhiteSpace())
                             .Select(x => x.Trim(' ', '\r', '\n'));
        }
    }
}
