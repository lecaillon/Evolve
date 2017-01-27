using System;
using System.Collections.Generic;
using System.Data;

namespace Evolve.Extensions
{
    public static class DbConnectionExtensions
    {
        public static int QueryForInt(this IDbConnection cnn, string sql)
        {
            throw new NotImplementedException();
        }

        //private  object Execute(IRelationalConnection connection, string executeMethod, IReadOnlyDictionary<string, object> parameterValues, bool closeConnection = true)
        //{
        //    Check.NotNull(connection, nameof(connection));
        //    Check.NotEmpty(executeMethod, nameof(executeMethod));

        //    var dbCommand = CreateCommand(connection, parameterValues);

        //    connection.Open();

        //    var startTimestamp = Stopwatch.GetTimestamp();
        //    var instanceId = Guid.NewGuid();

        //    DiagnosticSource.WriteCommandBefore(dbCommand, executeMethod, instanceId, startTimestamp, async: false);

        //    object result;
        //    try
        //    {
        //        switch (executeMethod)
        //        {
        //            case nameof(ExecuteNonQuery):
        //                {
        //                    using (dbCommand)
        //                    {
        //                        result = dbCommand.ExecuteNonQuery();
        //                    }

        //                    break;
        //                }
        //            case nameof(ExecuteScalar):
        //                {
        //                    using (dbCommand)
        //                    {
        //                        result = dbCommand.ExecuteScalar();
        //                    }

        //                    break;
        //                }
        //            case nameof(ExecuteReader):
        //                {
        //                    try
        //                    {
        //                        result = new RelationalDataReader(connection, dbCommand, dbCommand.ExecuteReader());
        //                    }
        //                    catch
        //                    {
        //                        dbCommand.Dispose();
        //                        throw;
        //                    }

        //                    break;
        //                }
        //            default:
        //                {
        //                    throw new NotSupportedException();
        //                }
        //        }

        //        var currentTimestamp = Stopwatch.GetTimestamp();

        //        Logger.LogCommandExecuted(dbCommand, startTimestamp, currentTimestamp);

        //        DiagnosticSource.WriteCommandAfter(dbCommand, executeMethod, instanceId, startTimestamp, currentTimestamp);

        //        if (closeConnection)
        //        {
        //            connection.Close();
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        var currentTimestamp = Stopwatch.GetTimestamp();

        //        Logger.LogCommandExecuted(dbCommand, startTimestamp, currentTimestamp);

        //        DiagnosticSource.WriteCommandError(dbCommand, executeMethod, instanceId, startTimestamp, currentTimestamp, exception, async: false);

        //        connection.Close();

        //        throw;
        //    }
        //    finally
        //    {
        //        dbCommand.Parameters.Clear();
        //    }

        //    return result;
        //}
    }
}
