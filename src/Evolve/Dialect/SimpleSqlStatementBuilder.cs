using System.Collections.Generic;

namespace Evolve.Dialect
{
    /// <summary>
    ///     A simple sql statement builder that does nothing and returns only one 
    ///     sql statement that must be enlists in a transacation.
    /// </summary>
    internal class SimpleSqlStatementBuilder : SqlStatementBuilderBase
    {
        public override string? BatchDelimiter => null;

        protected override IEnumerable<SqlStatement> Parse(string sqlScript, bool transactionEnabled)
        {
            if (sqlScript.IsNullOrWhiteSpace())
            {
                return new List<SqlStatement>();
            }

            return new[] { new SqlStatement(sqlScript, transactionEnabled) };
        }
    }
}
