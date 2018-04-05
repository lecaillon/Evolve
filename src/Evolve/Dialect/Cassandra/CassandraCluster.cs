using Evolve.Connection;
using Evolve.Metadata;

namespace Evolve.Dialect.Cassandra
{
    public sealed class CassandraCluster : DatabaseHelper
    {
        private string _currentKeyspaceName;
        
        public CassandraCluster(WrappedConnection wrappedConnection) : base(wrappedConnection)
        {
            _currentKeyspaceName = _currentKeyspaceName ?? GetFirstAvailableKeyspace(wrappedConnection);
        }

        public override string DatabaseName => "Cassandra";

        public override string CurrentUser => string.Empty;

        public override string GetCurrentSchemaName()
        {
            if (_currentKeyspaceName == null)
                _currentKeyspaceName = CassandraCluster.GetFirstAvailableKeyspace(WrappedConnection);

            return _currentKeyspaceName;
        }

        public static string GetFirstAvailableKeyspace(WrappedConnection wrappedConnection) =>
            wrappedConnection.QueryForString("select keyspace_name from system_schema.keyspaces limit 1");

        public override Schema GetSchema(string schemaName) => CassandraKeyspace.Retreive(schemaName, WrappedConnection);

        protected override void InternalChangeSchema(string toSchemaName)
        {
            WrappedConnection.ExecuteNonQuery($"Use {toSchemaName}");
            _currentKeyspaceName = toSchemaName;
        }

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) =>
            new CassandraMetadataTable(schema, tableName, this);

        /// <summary>
        ///     Will chekc for a predefined keyspace and table to see if there is a lock.
        ///     Otherwise, always returns true, because the lock is granted at table level.
        ///     <see cref="CassandraMetadataTable.TryLock"/>
        /// </summary>
        public override bool TryAcquireApplicationLock()
        {
            const string ClusterLockKeyspaceName = "cluster_lock";
            const string ClusterLockTableName = "lock";

            try
            {
                return WrappedConnection.QueryForLong($"select count(locked) from {ClusterLockKeyspaceName}.{ClusterLockTableName}") == 0;
            }
            catch (EvolveException ex)
                when (ex?.InnerException.GetType().ToString() == "Cassandra.InvalidQueryException")
            {
                //These error messages are very specific to Cassandra's drivers and could change
                if (ex.Message.StartsWith($"Keyspace {ClusterLockKeyspaceName} does not exist")
                    || ex.Message.StartsWith($"unconfigured table {ClusterLockTableName}"))
                    return true;
                else
                    return true;
            }
        }

        /// <summary>
        ///     Returns always true, because the lock is released at table level.
        ///     <see cref="CassandraMetadataTable.ReleaseLock"/>
        /// </summary>
        public override bool ReleaseApplicationLock() => true;

        public override SqlStatementBuilderBase SqlStatementBuilder { get; } = new CqlStatementBuilder();
    }
}
