using Evolve.Connection;
using Evolve.Dialect.PostgreSQL;
using Evolve.Metadata;

namespace Evolve.Dialect.CockroachDb
{
    public class CockroachDbDatabase : PostgreSQLDatabase
    {
        public CockroachDbDatabase(WrappedConnection wrappedConnection) : base(wrappedConnection)
        {
        }

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) =>
            new CockroachDbMetadataTable(schema, tableName, this);
    }
}