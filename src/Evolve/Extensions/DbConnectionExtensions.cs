using System;
using System.Collections.Generic;
using System.Data;
using Evolve.Utilities;

namespace Evolve.Extensions
{
    public static class DbConnectionExtensions
    {
        private const string CommandExecutionError = "DbCommand ({0}) error: {1}";

        public static long QueryForLong(this IDbConnection connection, string sql) => (long)ExecuteScalar(connection, sql);

        public static string QueryForString(this IDbConnection connection, string sql) => (string)ExecuteScalar(connection, sql);

        public static IEnumerable<string> QueryForListOfString(this IDbConnection connection, string sql)
        {
            var list = new List<string>();
            using (var reader = (IDataReader)ExecuteReader(connection, sql))
            {
                while (reader.Read())
                {
                    list.Add(reader[0] is DBNull ? null : reader[0].ToString());
                }
            }

            return list;
        }

        static int ExecuteNonQuery(IDbConnection connection, string sql, IDbTransaction transaction = null)
            => (int)Execute(connection, sql, transaction, nameof(ExecuteNonQuery));

        static object ExecuteScalar(IDbConnection connection, string sql, IDbTransaction transaction = null)
            => Execute(connection, sql, transaction, nameof(ExecuteScalar));

        static object ExecuteReader(IDbConnection connection, string sql, IDbTransaction transaction = null)
            => Execute(connection, sql, transaction, nameof(ExecuteReader));

        static object Execute(IDbConnection connection, string sql, IDbTransaction transaction, string executeMethod)
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
                throw new EvolveException(string.Format(CommandExecutionError, executeMethod, sql), ex);
            }

            return result;
        }

    }
}
