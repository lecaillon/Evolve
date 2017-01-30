using System;
using System.Data;

namespace Evolve.Connection
{
    public interface IWrappedConnection : IDisposable
    {
        /// <summary>
        ///     Gets the underlying <see cref="IDbConnection" /> used to connect to the database.
        /// </summary>
        IDbConnection DbConnection { get; }

        /// <summary>
        ///     Gets the current transaction.
        /// </summary>
        IDbTransaction CurrentTx { get; }

        /// <summary>
        ///     Gets or sets the timeout for executing a command against the database.
        /// </summary>
        int? CommandTimeout { get; set; }

        /// <summary>
        ///     Opens the connection to the database.
        /// </summary>
        void Open();

        /// <summary>
        ///     Closes the connection to the database.
        /// </summary>
        void Close();

        /// <summary>
        ///     Begins a new transaction.
        /// </summary>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <returns> The newly created transaction. </returns>
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        ///     Commits all changes made to the database in the current transaction.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Discards all changes made to the database in the current transaction.
        /// </summary>
        void Rollback();
    }
}
