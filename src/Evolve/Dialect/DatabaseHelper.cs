using System.Data;
using Evolve.Connection;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Dialect
{
    public abstract class DatabaseHelper
    {
        private const string SchemaNotFound = "Cannot change schema to {0}. This schema does not exist.";
        protected readonly string _originalSchemaName;

        public DatabaseHelper(WrappedConnection wrappedConnection)
        {
            WrappedConnection = Check.NotNull(wrappedConnection, nameof(wrappedConnection));
            _originalSchemaName = GetCurrentSchemaName();
        }

        public WrappedConnection WrappedConnection { get; }

        public abstract string DatabaseName { get; }

        public abstract string CurrentUser { get; }

        public abstract string BatchDelimiter { get; }

        public abstract SqlStatementBuilder SqlStatementBuilder { get; }

        public virtual Schema ChangeSchema(string toSchemaName)
        {
            var schema = GetSchema(toSchemaName);
            if (!schema.IsExists())
                throw new EvolveException(string.Format(SchemaNotFound, toSchemaName));

            InternalChangeSchema(toSchemaName);
            return schema;
        }

        public virtual void RestoreOriginalSchema() => ChangeSchema(_originalSchemaName);

        public abstract string GetCurrentSchemaName();

        public abstract Schema GetSchema(string schemaName);

        protected abstract void InternalChangeSchema(string toSchemaName);

        public abstract IEvolveMetadata GetMetadataTable(string schema, string tableName);

        public abstract bool TryAcquireApplicationLock();

        public abstract bool ReleaseApplicationLock();

        public virtual void CloseConnection()
        {
            if (WrappedConnection.DbConnection.State != ConnectionState.Closed)
            {
                WrappedConnection.DbConnection.Close();
            }
        }
    }
}
