using Evolve.Connection;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Dialect
{
    public abstract class DatabaseHelper
    {
        protected string _originalSchemaName;

        public DatabaseHelper(IWrappedConnection wrappedConnection)
        {
            WrappedConnection = Check.NotNull(wrappedConnection, nameof(wrappedConnection));
            _originalSchemaName = GetCurrentSchemaName();
        }

        public IWrappedConnection WrappedConnection { get; private set; }

        public abstract string DatabaseName { get; }

        #region Schema helper

        public virtual Schema ChangeSchema(string toSchemaName)
        {
            var schema = GetSchema(toSchemaName);
            if (!schema.IsExists())
                return null;

            InternalChangeSchema(toSchemaName);
            return schema;
        }

        public virtual void RestoreOriginalSchema() => ChangeSchema(_originalSchemaName);

        public abstract string GetCurrentSchemaName();

        public abstract void InternalChangeSchema(string toSchemaName);

        protected abstract Schema GetSchema(string schemaName);

        #endregion

        #region MetadataTable helper

        public abstract IEvolveMetadata GetMetadataTable(string schema, string tableName);

        #endregion
    }
}
