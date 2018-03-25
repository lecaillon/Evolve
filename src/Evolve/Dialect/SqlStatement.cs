namespace Evolve.Dialect
{
    public class SqlStatement
    {
        /// <summary>
        ///     Initialize a instance of the <see cref="SqlStatement"/> class.
        /// </summary>
        public SqlStatement(string sql, int lineNumber, bool mustExecuteInTransaction)
        {
            Sql = sql;
            LineNumber = lineNumber;
            MustExecuteInTransaction = mustExecuteInTransaction;
        }

        /// <summary>
        ///     Gets the line number where this statement is located in the script.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        ///     Gets the SQL statement to execute.
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        ///     Returns true if the statement must be executed inside a transaction, false otherwise.
        /// </summary>
        public bool MustExecuteInTransaction { get; set; }
    }
}
