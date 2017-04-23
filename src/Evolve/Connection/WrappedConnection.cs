using System;
using System.Data;
using Evolve.Utilities;

namespace Evolve.Connection
{
    /// <summary>
    ///     A wrapper of <see cref="IDbConnection"/> used to managed all the queries and transactions to the database to evolve.
    /// </summary>
    public class WrappedConnection : IDisposable
    {
        private readonly bool _connectionOwned;
        private int _openedCount;
        private bool _openedInternally;
        private const string TransactionAlreadyStarted = "The connection is already in a transaction and cannot participate in another transaction.";
        private const string NoActiveTransaction = "The connection does not have any active transactions.";
        private const string ConnectionValidationError = "Validation of the database connection failed.";

        /// <summary>
        ///     Initializes a new instance of <see cref="WrappedConnection"/>.
        /// </summary>
        /// <param name="connection"> The connection used to interact with the database. </param>
        /// <param name="connectionOwned"> true if Evolve is responsible of disposing the underlying connection, otherwise false. </param>
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
        ///     Return true if we are connected to an in-memomry SQLite database, false otherwisee.
        /// </summary>
        public bool SQLiteInMemoryDatabase => DbConnection.ConnectionString.Contains(":memory:");

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

            if(CurrentTx.Connection != null) // Check if tx is not already completed
            {
                CurrentTx.Rollback();
            }
            
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
            if (_openedCount > 0 && --_openedCount == 0 && _openedInternally && !SQLiteInMemoryDatabase)
            {
                if (DbConnection.State != ConnectionState.Closed)
                {
                    DbConnection.Close();
                }
                _openedInternally = false;
            }
        }

        /// <summary>
        ///     Validate the database connection by opening and closing it.
        /// </summary>
        /// <exception cref="EvolveException"> Throws EvolveException if validation fails. </exception>
        public void Validate()
        {
            try
            {
                if (DbConnection.State == ConnectionState.Open)
                {
                    return;
                }

                Open();
                Close();
            }
            catch (Exception ex)
            {
                throw new EvolveException(ConnectionValidationError, ex);
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
