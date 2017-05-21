using System;
using Evolve.Connection;
using Evolve.Metadata;

namespace Evolve.Dialect.MySQL
{
    public class MySQLDatabase : DatabaseHelper
    {
        public MySQLDatabase(WrappedConnection wrappedConnection) : base(wrappedConnection)
        {
        }

        public override string DatabaseName => "MySQL";

        public override string CurrentUser => "SUBSTRING_INDEX(USER(),'@',1)";

        public override string BatchDelimiter => null;

        public override string GetCurrentSchemaName() => WrappedConnection.QueryForString("SELECT DATABASE();");

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) => new MySQLMetadataTable(schema, tableName, this);

        public override Schema GetSchema(string schemaName) => new MySQLSchema(schemaName, WrappedConnection);

        protected override void InternalChangeSchema(string toSchemaName)
        {
            if (toSchemaName.IsNullOrWhiteSpace())
            {
                // Hack to switch back to no database selected...
                String tempDb = "`" + Guid.NewGuid() + "`";
                WrappedConnection.ExecuteNonQuery($"CREATE SCHEMA {tempDb}");
                WrappedConnection.ExecuteNonQuery($"USE {tempDb}");
                WrappedConnection.ExecuteNonQuery($"DROP SCHEMA {tempDb}");
            }
            else
            {
                WrappedConnection.ExecuteNonQuery($"USE `{toSchemaName}`");
            }
        }
    }
}
