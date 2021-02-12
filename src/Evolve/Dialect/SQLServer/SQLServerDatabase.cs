using System.Data;
using System.Diagnostics.CodeAnalysis;
using Evolve.Connection;
using Evolve.Metadata;

namespace Evolve.Dialect.SQLServer
{
    internal class SQLServerDatabase : DatabaseHelper
    {
        private const string LOCK_ID = "Evolve";

        public SQLServerDatabase(WrappedConnection wrappedConnection) : base(wrappedConnection)
        {
        }

        public override string DatabaseName => "SQL Server";

        public override string CurrentUser => "SUSER_SNAME()";

        public override SqlStatementBuilderBase SqlStatementBuilder => new SQLServerStatementBuilder();

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) => new SQLServerMetadataTable(schema, tableName, this);

        public override Schema GetSchema(string schemaName) => new SQLServerSchema(schemaName, WrappedConnection);

        public override bool TryAcquireApplicationLock()
        {
            return WrappedConnection.ExecuteDbCommand("sp_getapplock", cmd =>
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var outParam = cmd.CreateParameter();
                outParam.ParameterName = "@result";
                outParam.DbType = DbType.Int32;
                outParam.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(outParam);

                var inParam1 = cmd.CreateParameter();
                inParam1.ParameterName = "@Resource";
                inParam1.Value = LOCK_ID;
                inParam1.DbType = DbType.String;
                inParam1.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(inParam1);

                var inParam2 = cmd.CreateParameter();
                inParam2.ParameterName = "@LockOwner";
                inParam2.Value = "Session";
                inParam2.DbType = DbType.String;
                inParam2.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(inParam2);

                var inParam3 = cmd.CreateParameter();
                inParam3.ParameterName = "@LockMode";
                inParam3.Value = "Exclusive";
                inParam3.DbType = DbType.String;
                inParam3.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(inParam3);

                var inParam4 = cmd.CreateParameter();
                inParam4.ParameterName = "@LockTimeout";
                inParam4.Value = 0;
                inParam4.DbType = DbType.Int32;
                inParam4.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(inParam4);

            }, cmd =>
            {
                cmd.ExecuteNonQuery();
                return (int)(cmd.Parameters["@result"] as IDbDataParameter)!.Value!;
            }) >= 0;
        }

        public override bool ReleaseApplicationLock()
        {
            return WrappedConnection.ExecuteDbCommand("sp_releaseapplock", cmd =>
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var outParam = cmd.CreateParameter();
                outParam.ParameterName = "@result";
                outParam.DbType = DbType.Int32;
                outParam.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(outParam);

                var inParam1 = cmd.CreateParameter();
                inParam1.ParameterName = "@Resource";
                inParam1.Value = LOCK_ID;
                inParam1.DbType = DbType.String;
                inParam1.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(inParam1);

                var inParam2 = cmd.CreateParameter();
                inParam2.ParameterName = "@LockOwner";
                inParam2.Value = "Session";
                inParam2.DbType = DbType.String;
                inParam2.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(inParam2);

            }, cmd =>
            {
                cmd.ExecuteNonQuery();
                return (int)(cmd.Parameters["@result"] as IDbDataParameter)!.Value!;
            }) >= 0;
        }


        /// <summary>
        ///     In SQL Server, when using Windows Groups as a database user, it is possible to set the DEFAULT_SCHEMA to NULL.
        ///     https://github.com/lecaillon/Evolve/issues/156
        /// </summary>
        [SuppressMessage("Design", "CA1031: Do not catch general exception types")]
        public override string? GetCurrentSchemaName()
        {
            try
            {
                return WrappedConnection.QueryForString("SELECT SCHEMA_NAME()");
            }
            catch
            {
                return null;
            }
        }
    }
}
