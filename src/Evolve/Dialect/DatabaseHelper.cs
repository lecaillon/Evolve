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

        #region Query helper

        public virtual long QueryForLong(string sql) => (long)ExecuteScalar(Connection, sql);

        public virtual string QueryForString(string sql) => (string)ExecuteScalar(Connection, sql);

        public virtual IEnumerable<string> QueryForListOfString(string sql)
        {
            var list = new List<string>();
            using (var reader = (IDataReader)ExecuteReader(Connection, sql))
            {
                while (reader.Read())
                {
                    list.Add(reader[0] is DBNull ? null : reader[0].ToString());
                }
            }

            return list;
        }

        protected int ExecuteNonQuery(IDbConnection connection, string sql, IDbTransaction transaction = null)
            => (int)Execute(connection, sql, transaction, nameof(ExecuteNonQuery));

        protected object ExecuteScalar(IDbConnection connection, string sql, IDbTransaction transaction = null)
            => Execute(connection, sql, transaction, nameof(ExecuteScalar));

        protected object ExecuteReader(IDbConnection connection, string sql, IDbTransaction transaction = null)
            => Execute(connection, sql, transaction, nameof(ExecuteReader));

        protected object Execute(IDbConnection connection, string sql, IDbTransaction transaction, string executeMethod)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNullOrEmpty(sql, nameof(sql));
            Check.NotNullOrEmpty(executeMethod, nameof(executeMethod));

            bool wasClosed = connection.State == ConnectionState.Closed;
            var dbCommand = connection.CreateCommand();
            dbCommand.CommandText = sql;
            dbCommand.Transaction = transaction;

            object result;
            try
            {
                if (wasClosed) connection.Open();

                switch (executeMethod)
                {
                    case nameof(ExecuteNonQuery):
                    {
                        using (dbCommand)
                        {
                            result = dbCommand.ExecuteNonQuery();
                        }

                        break;
                    }
                    case nameof(ExecuteScalar):
                    {
                        using (dbCommand)
                        {
                            result = dbCommand.ExecuteScalar();
                        }

                        break;
                    }
                    case nameof(ExecuteReader):
                    {
                        using (dbCommand)
                        {
                            result = dbCommand.ExecuteReader();
                        }

                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException();
                    }
                }

                if (wasClosed)
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                connection.Close();
                throw new EvolveException("", ex);
            }

            return result;
        }

        #endregion
    }
}
