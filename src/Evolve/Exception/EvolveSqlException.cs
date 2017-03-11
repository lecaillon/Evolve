using System;

namespace Evolve
{
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
