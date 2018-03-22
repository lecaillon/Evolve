using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Dialect
{
    public class SqlStatement
    {
        public SqlStatement()
        {

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
        public bool CanExecuteInTransaction { get; set; }
    }
}
