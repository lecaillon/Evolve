using Evolve.Connection;
using Evolve.Utilities;

namespace Evolve.Dialect
{
    public abstract class DatabaseHelper
    {
        public DatabaseHelper(IWrappedConnection wrappedConnection)
        {
            WrappedConnection = Check.NotNull(wrappedConnection, nameof(wrappedConnection));
            OriginalSchemaName = InternalGetCurrentSchemaName();
        }

        public IWrappedConnection WrappedConnection { get; private set; }

        public abstract string DatabaseName { get; protected set; }

        #region Schema helper

        public string OriginalSchemaName { get; private set; }

        public string GetSchemaName()
        {
            // + gestion exception
            return InternalGetCurrentSchemaName();
        }

        public void RestoreSchema()
        {
            // + gestion exception
            InternalRestoreCurrentSchema();
        }

        public abstract Schema GetSchema(string schemaName);

        protected abstract string InternalGetCurrentSchemaName();

        protected abstract string InternalRestoreCurrentSchema();

        #endregion
    }
}
