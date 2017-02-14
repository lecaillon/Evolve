using Evolve.Utilities;
using System;
using System.Data;

namespace Evolve.Connection
{
    public class WrappedConnection : IDisposable
    {
        private readonly bool _connectionOwned;
        private int _openedCount;
        private bool _openedInternally;
        private const string TransactionAlreadyStarted = "The connection is already in a transaction and cannot participate in another transaction.";
        private const string NoActiveTransaction = "The connection does not have any active transactions.";

        public WrappedConnection(IDbConnection connection, bool connectionOwned = true)
        {
            DbConnection = Check.NotNull(connection, nameof(connection));
            _connectionOwned = connectionOwned;
        }

        /// <summary>
        ///     Gets the underlying <see cref="IDbConnection" /> used to connect to the database.
        /// </summary>
        public IDbConnection DbConnection { get; }

        /// <summary>
        ///     Gets the current transaction.
        /// </summary>
        public IDbTransaction CurrentTx { get; private set; }

        /// <summary>
        ///     Begins a new transaction.
        /// </summary>
        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (CurrentTx != null)
            {
                throw new InvalidOperationException(TransactionAlreadyStarted);
            }

            Open();

            CurrentTx = DbConnection.BeginTransaction(isolationLevel);
            return CurrentTx;
        }

        /// <summary>
        ///     Commits all changes made to the database in the current transaction.
        /// </summary>
        public void Commit()
        {
            if (CurrentTx == null)
            {
                throw new InvalidOperationException(NoActiveTransaction);
            }

            CurrentTx.Commit();
            ClearTransaction();
        }

        /// <summary>
        ///     Discards all changes made to the database in the current transaction.
        /// </summary>
        public void Rollback()
        {
            if (CurrentTx == null)
            {
                throw new InvalidOperationException(NoActiveTransaction);
            }

            CurrentTx.Rollback();
            ClearTransaction();
        }

        /// <summary>
        ///     Opens the connection to the database.
        /// </summary>
        public void Open()
        {
            if (DbConnection.State == ConnectionState.Broken)
            {
                DbConnection.Close();
            }

            if (DbConnection.State != ConnectionState.Open)
            {
                DbConnection.Open();

                if (_openedCount == 0)
                {
                    _openedInternally = true;
                    _openedCount++;
                }
            }
            else
            {
                _openedCount++;
            }
        }

        /// <summary>
        ///     Closes the connection to the database.
        /// </summary>
        public void Close()
        {
            if (_openedCount > 0 && --_openedCount == 0 && _openedInternally)
            {
                if (DbConnection.State != ConnectionState.Closed)
                {
                    DbConnection.Close();
                }
                _openedInternally = false;
            }
        }

        public void Dispose()
        {
            CurrentTx?.Dispose();

            if (_connectionOwned)
            {
                DbConnection.Dispose();
                _openedCount = 0;
            }
        }

        private void ClearTransaction()
        {
            CurrentTx = null;
            Close();
        }
    }
}
