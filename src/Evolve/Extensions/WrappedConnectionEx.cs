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
                dbVersion = QueryForString(wrappedConnection, "SHOW VARIABLES LIKE '%version%';");
                if (!dbVersion.IsNullOrWhiteSpace())
                {
                    return DBMS.MySQL_MariaDB;
                }
            }
            catch { }

            try
            {
                dbVersion = QueryForString(wrappedConnection, "SELECT version()"); // attention ca marche aussi pour mysql
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

        public static long QueryForLong(this WrappedConnection wrappedConnection, string sql)
        {
            return Execute(wrappedConnection, sql, (cmd) =>
            {
                return Convert.ToInt64(cmd.ExecuteScalar());
            });
        }

        public static string QueryForString(this WrappedConnection wrappedConnection, string sql)
        {
            return Execute(wrappedConnection, sql, (cmd) =>
            {
                return (string)cmd.ExecuteScalar();
            });
        }

        public static IEnumerable<string> QueryForListOfString(this WrappedConnection wrappedConnection, string sql)
        {
            return Execute(wrappedConnection, sql, (cmd) =>
            {
                var list = new List<string>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader[0] is DBNull ? null : reader[0].ToString());
                    }
                }

                return list;
            });
        }

        public static IEnumerable<T> QueryForList<T>(this WrappedConnection wrappedConnection, string sql, Func<IDataReader, T> map)
        {
            return Execute(wrappedConnection, sql, (cmd) =>
            {
                var list = new List<T>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(map(reader));
                    }
                }

                return list;
            });
        }

        public static int ExecuteNonQuery(this WrappedConnection wrappedConnection, string sql)
        {
            return Execute(wrappedConnection, sql, (cmd) =>
            {
                return cmd.ExecuteNonQuery();
            });
        }

        private static T Execute<T>(WrappedConnection wrappedConnection, string sql, Func<IDbCommand, T> query)
        {
            Check.NotNull(wrappedConnection, nameof(wrappedConnection));
            Check.NotNullOrEmpty(sql, nameof(sql));
            Check.NotNull(query, nameof(query));

            bool wasClosed = wrappedConnection.DbConnection.State == ConnectionState.Closed;

            try
            {
                if (wasClosed)
                {
                    wrappedConnection.Open();
                }

                using (IDbCommand cmd = wrappedConnection.DbConnection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Transaction = wrappedConnection.CurrentTx;

                    return query(cmd);
                }
            }
            catch (Exception ex)
            {
                throw new EvolveSqlException(sql, ex);
            }
            finally
            {
                if (wasClosed)
                {
                    wrappedConnection.Close();
                }
            }
        }
    }
}
