using Evolve.Connection;
using Evolve.Metadata;

namespace Evolve.Dialect.PostgreSQL
{
    public class PostgreSQLDatabase : DatabaseHelper
    {
        public PostgreSQLDatabase(WrappedConnection wrappedConnection) : base(wrappedConnection)
        {
        }

        public override string DatabaseName => "PostgreSQL";

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) => new PostgreSQLMetadataTable(schema, tableName, WrappedConnection);

        public override string GetCurrentSchemaName() => WrappedConnection.QueryForString("SHOW search_path");

        protected override Schema GetSchema(string schemaName) => new PostgreSQLSchema(schemaName, WrappedConnection);

        protected override void InternalChangeSchema(string toSchemaName)
        {
            if(string.IsNullOrWhiteSpace(toSchemaName))
            {
                WrappedConnection.ExecuteNonQuery("SELECT set_config('search_path', '', false)");
            }
            else
            {
                WrappedConnection.ExecuteNonQuery($"SET search_path = \"{toSchemaName}\"");
            }
        }
    }
}
