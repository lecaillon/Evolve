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

        public override string GetCurrentSchemaName() => _currentKeyspaceName;

        public override Schema GetSchema(string schemaName) => CassandraKeyspace.Retreive(schemaName, WrappedConnection);

        protected override void InternalChangeSchema(string toSchemaName)
        {
            WrappedConnection.ExecuteNonQuery($"Use {toSchemaName}");
            _currentKeyspaceName = toSchemaName;
        }

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) =>
            new CassandraMetadataTable(schema, tableName, this);

        /// <summary>
        ///     Returns always true, because the lock is granted at table level.
        ///     <see cref="CassandraMetadataTable.TryLock"/>
        /// </summary>
        public override bool TryAcquireApplicationLock() => true;

        /// <summary>
        ///     Returns always true, because the lock is released at table level.
        ///     <see cref="CassandraMetadataTable.ReleaseLock"/>
        /// </summary>
        public override bool ReleaseApplicationLock() => true;

        public override SqlStatementBuilderBase SqlStatementBuilder { get; } = new CqlStatementBuilder();
    }
}
