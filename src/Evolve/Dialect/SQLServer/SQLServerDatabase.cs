using Evolve.Connection;
using Evolve.Metadata;

namespace Evolve.Dialect.SQLServer
{
    public class SQLServerDatabase : DatabaseHelper
    {
        public SQLServerDatabase(WrappedConnection wrappedConnection) : base(wrappedConnection)
        {
        }

        public override string DatabaseName => "SQL Server";

        public override string CurrentUser => "SUSER_SNAME()";

        public override string BatchDelimiter => "GO";

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) => new SQLServerMetadataTable(schema, tableName, this);

        public override Schema GetSchema(string schemaName) => new SQLServerSchema(schemaName, WrappedConnection);

        public override string GetCurrentSchemaName() => WrappedConnection.QueryForString("SELECT SCHEMA_NAME()");

        /// <summary>
        ///     SQL Server does not support changing the schema in a session.
        /// </summary>
        protected override void InternalChangeSchema(string toSchemaName) { }
    }
}
