namespace Evolve.Dialect
{
    /// <summary>
    ///     A SQL statement from a script that can be executed at once against a database.
    /// </summary>
    internal class SqlStatement
    {
        /// <summary>
        ///     Initialize a instance of the <see cref="SqlStatement"/> class.
        /// </summary>
        public SqlStatement(string sql, bool mustExecuteInTransaction, int lineNumber = -1)
        {
            Sql = sql;
            MustExecuteInTransaction = mustExecuteInTransaction;
            LineNumber = lineNumber;
        }

        /// <summary>
        ///     Gets the line number where this statement is located in the script.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        ///     Gets the SQL statement to execute.
        /// </summary>
        public string Sql { get; }

        /// <summary>
        ///     Returns true if the statement must be executed inside a transaction, false otherwise.
        /// </summary>
        public bool MustExecuteInTransaction { get; }
    }
}
