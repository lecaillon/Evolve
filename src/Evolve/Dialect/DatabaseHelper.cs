using System;
using System.Collections.Generic;
using System.Data;
using Evolve.Connection;
using Evolve.Utilities;

namespace Evolve.Dialect
{
    public abstract class DatabaseHelper
    {
        public DatabaseHelper(IConnectionProvider connectionProvider)
        {
            Check.NotNull(connectionProvider, nameof(connectionProvider));

            ConnectionProvider = connectionProvider;
            OriginalSchemaName = InternalGetCurrentSchemaName();
        }

        public IConnectionProvider ConnectionProvider { get; private set; }

        protected IDbConnection Connection => ConnectionProvider.GetConnection();

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
