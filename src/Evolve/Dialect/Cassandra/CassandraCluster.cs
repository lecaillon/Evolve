using System.Data;
using Evolve.Connection;
using Evolve.Metadata;

namespace Evolve.Dialect.Cassandra
{
    public sealed class CassandraCluster : DatabaseHelper
    {
        private string _currentKeyspaceName = "system";

        public CassandraCluster(WrappedConnection wrappedConnection) : base(wrappedConnection) { }

        public override string DatabaseName => "Cassandra";

        public override string CurrentUser => string.Empty;

        public override string BatchDelimiter => null;

        public override string GetCurrentSchemaName() => _currentKeyspaceName;

        public override Schema GetSchema(string schemaName) => CassandraKeyspace.Retreive(schemaName, WrappedConnection);

        protected override void InternalChangeSchema(string toSchemaName)
        {
            WrappedConnection.ExecuteNonQuery($"Use {toSchemaName}");
            _currentKeyspaceName = toSchemaName;
        }

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) =>
            new CassandraMetadataTable(schema, tableName, this);

        public override bool TryAcquireApplicationLock() => true;

        public override bool ReleaseApplicationLock() => true;
    }
}
