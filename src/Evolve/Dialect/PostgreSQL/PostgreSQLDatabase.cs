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

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) => new PostgreSQLMetadataTable(schema, tableName, this);

        public override string GetCurrentSchemaName() => CleanSchemaName(WrappedConnection.QueryForString("SHOW search_path"));

        public override Schema GetSchema(string schemaName) => new PostgreSQLSchema(schemaName, WrappedConnection);

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

        private string CleanSchemaName(string schemaName)
        {
            if(string.IsNullOrWhiteSpace(schemaName))
            {
                return string.Empty;
            }

            string newSchemaName = schemaName.Replace("\"", "")
                                             .Replace("$user", "")
                                             .Trim();

            if(newSchemaName.StartsWith(","))
            {
                newSchemaName = newSchemaName.Substring(1);
            }

            if (newSchemaName.Contains(","))
            {
                newSchemaName = newSchemaName.Substring(0, newSchemaName.IndexOf(","));
            }

            return newSchemaName.Trim();
        }
    }
}
