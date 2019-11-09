using System;
using System.Diagnostics.CodeAnalysis;

namespace Evolve
{
    [SuppressMessage("Design", "CA1032: Implement standard exception constructors")]
    public class EvolveSqlException : EvolveException
    {
        public EvolveSqlException(string sql, Exception innerEx) 
            : base($"{innerEx.Message} Sql query: {sql.Replace(Environment.NewLine, " ").TruncateWithEllipsis(100)}", innerEx)
        {
            Sql = sql;
        }

        public string Sql { get; }
    }
}
