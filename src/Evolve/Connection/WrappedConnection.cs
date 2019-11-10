using System;
using System.Data;
using Evolve.Utilities;

namespace Evolve.Connection
{
    /// <summary>
    ///     A wrapper of <see cref="IDbConnection"/> used to managed all the queries and transactions to the database to evolve.
    /// </summary>
    internal class WrappedConnection : IDisposable
    {
        private const string TransactionAlreadyStarted = "The connection is already in a transaction and cannot participate in another transaction.";
        private const string NoActiveTransaction = "The connection does not have any active transactions.";
        private const string ConnectionValidationError = "Validation of the database connection failed.";
        private bool _openedInternally = false;
        private bool _disposedValue = false;

        /// <summary>
        ///     Initializes a new instance of <see cref="WrappedConnection"/>.
        /// </summary>
        /// <param name="connection"> The connection used to interact with the database. </param>
        public WrappedConnection(IDbConnection connection)
        {
            DbConnection = Check.NotNull(connection, nameof(connection));
        }

        /// <summary>
        ///     Gets the underlying <see cref="IDbConnection" /> used to connect to the database.
        /// </summary>
        public IDbConnection DbConnection { get; }

        /// <summary>
        ///     Gets the current transaction.
        /// </summary>
        public IDbTransaction? CurrentTx { get; private set; }

        /// <summary>
        ///     Return true if we are connected to an in-memomry SQLite database, false otherwise.
        /// </summary>
        public bool SQLiteInMemoryDatabase => DbConnection.ConnectionString.Contains(":memory:");

        /// <summary>
        ///     Returns true if we are connected to a Cassandra cluster, false otherwise.
        /// </summary>
        internal bool CassandraCluster => DbConnection.ConnectionString.Contains("contact points=");

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
            if (CurrentTx is null)
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
            if (CurrentTx is null)
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
                _openedInternally = true;
            }
        }

        /// <summary>
        ///     Never close the connection since it will release the database 
        ///     lock used to prevent concurrent execution of Evolve.
        /// </summary>
        public void Close()
        {
            if (_openedInternally && !SQLiteInMemoryDatabase)
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
        public WrappedConnection Validate()
        {
            try
            {
                if (DbConnection.State == ConnectionState.Open)
                {
                    return this;
                }

                Open();
                Close();

                return this;
            }
            catch (Exception ex)
            {
                throw new EvolveException(ConnectionValidationError, ex);
            }
        }

        /// <summary>
        ///     Close the underlying connection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    ClearTransaction();
                    Close();
                }

                _disposedValue = true;
            }
        }

        private void ClearTransaction()
        {
            CurrentTx?.Dispose();
            CurrentTx = null;
        }
    }
}
