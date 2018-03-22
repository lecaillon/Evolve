using System;
using System.Collections.Generic;
using Evolve.Migration;

namespace Evolve.Dialect
{
    public class SimpleSqlStatementBuilder : SqlStatementBuilder
    {
        protected override IEnumerable<SqlStatement> Parse(string sqlScript, string delimiter)
        {
            throw new NotImplementedException();
        }
    }
}
