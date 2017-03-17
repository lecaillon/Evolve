using System;
using System.Collections.Generic;
using System.Data;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Utilities;

namespace Evolve
{
    public static class WrappedConnectionEx
    {
        private const string DBMSNotSUpported = "Connection to this DBMS is not supported.";

        public static DBMS GetDatabaseServerType(this WrappedConnection wrappedConnection)
        {
            string dbVersion = null;

            try
            {
                dbVersion = QueryForString(wrappedConnection, "SELECT version()"); // attention ca marche aussi pour mysql/mariadb ...
                if (!dbVersion.IsNullOrWhiteSpace()) 
                {
                    return DBMS.PostgreSQL;
                }
            }
            catch { }

            try
            {
                dbVersion = QueryForString(wrappedConnection, "SELECT @@version");
                if (!dbVersion.IsNullOrWhiteSpace())
                {
                    return DBMS.SQLServer;
                }
            }
            catch { }

            try
            {
                dbVersion = QueryForString(wrappedConnection, "SELECT sqlite_version()");
                if (!dbVersion.IsNullOrWhiteSpace())
                {
                    return DBMS.SQLite;
                }
            }
            catch { }

            throw new EvolveException(DBMSNotSUpported);
        }

        public static long QueryForLong(this WrappedConnection wrappedConnection, string sql) => Convert.ToInt64(ExecuteScalar(wrappedConnection, sql));

        public static string QueryForString(this WrappedConnection wrappedConnection, string sql) => (string)ExecuteScalar(wrappedConnection, sql);

        public static IEnumerable<string> QueryForListOfString(this WrappedConnection wrappedConnection, string sql)
        {
            Check.NotNull(wrappedConnection, nameof(wrappedConnection));
            Check.NotNullOrEmpty(sql, nameof(sql));

            var list = new List<string>();
            bool wasClosed = wrappedConnection.DbConnection.State == ConnectionState.Closed;

            try
            {
                using (var reader = (IDataReader)ExecuteReader(wrappedConnection, sql))
                {
                    while (reader.Read())
                    {
                        list.Add(reader[0] is DBNull ? null : reader[0].ToString());
                    }
                }
            }
            finally
            {
                if (wasClosed)
                {
                    wrappedConnection.Close();
                }
            }

            return list;
        }

        public static IEnumerable<T> QueryForList<T>(this WrappedConnection wrappedConnection, string sql, Func<IDataReader, T> map)
        {
            Check.NotNull(wrappedConnection, nameof(wrappedConnection));
            Check.NotNullOrEmpty(sql, nameof(sql));
            Check.NotNull(map, nameof(map));

            var list = new List<T>();
            bool wasClosed = wrappedConnection.DbConnection.State == ConnectionState.Closed;

            try
            {
                using (var reader = (IDataReader)ExecuteReader(wrappedConnection, sql))
                {
                    while (reader.Read())
                    {
                        list.Add(map(reader));
                    }
                }
            }
            finally
            {
                if (wasClosed)
                {
                    wrappedConnection.Close();
                }
            }

            return list;
        }

        public static int ExecuteNonQuery(this WrappedConnection wrappedConnection, string sql)
            => (int)Execute(wrappedConnection, sql, nameof(ExecuteNonQuery));

        private static object ExecuteScalar(WrappedConnection wrappedConnection, string sql)
            => Execute(wrappedConnection, sql, nameof(ExecuteScalar));

        private static object ExecuteReader(WrappedConnection wrappedConnection, string sql)
            => Execute(wrappedConnection, sql, nameof(ExecuteReader), closeConnection: false);

        private static object Execute(WrappedConnection wrappedConnection, string sql, string executeMethod, bool closeConnection = true)
        {
            Check.NotNull(wrappedConnection, nameof(wrappedConnection));
            Check.NotNullOrEmpty(sql, nameof(sql));
            Check.NotNullOrEmpty(executeMethod, nameof(executeMethod));
            
            bool wasClosed = wrappedConnection.DbConnection.State == ConnectionState.Closed;
            var dbCommand = wrappedConnection.DbConnection.CreateCommand();
            dbCommand.CommandText = sql;
            dbCommand.Transaction = wrappedConnection.CurrentTx;

            object result;
            try
            {
                if (wasClosed) wrappedConnection.Open();

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

                return result;
            }
            catch (Exception ex)
            {
                throw new EvolveSqlException(sql, ex);
            }
            finally
            {
                if (wasClosed && closeConnection)
                {
                    wrappedConnection.Close();
                }
            }
        }
    }
}
