using System;
using Evolve.Connection;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Dialect
{
    internal abstract class DatabaseHelper : IDisposable
    {
        private bool _disposedValue = false;

        protected DatabaseHelper(WrappedConnection wrappedConnection)
        {
            WrappedConnection = Check.NotNull(wrappedConnection, nameof(wrappedConnection));
        }

        public WrappedConnection WrappedConnection { get; }

        public abstract string DatabaseName { get; }

        public abstract string CurrentUser { get; }
        
        public abstract SqlStatementBuilderBase SqlStatementBuilder { get; }

        public abstract string? GetCurrentSchemaName();

        public abstract Schema GetSchema(string schemaName);

        public abstract IEvolveMetadata GetMetadataTable(string schema, string tableName);

        public abstract bool TryAcquireApplicationLock();

        public abstract bool ReleaseApplicationLock();

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
                    WrappedConnection.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
