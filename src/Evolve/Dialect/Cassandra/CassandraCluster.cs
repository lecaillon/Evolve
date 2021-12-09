using System;
using System.Linq;
using EvolveDb.Connection;
using EvolveDb.Metadata;
using SimpleJSON;

namespace EvolveDb.Dialect.Cassandra
{
    internal sealed class CassandraCluster : DatabaseHelper
    {
        private string _currentKeyspaceName;
        
        public CassandraCluster(WrappedConnection wrappedConnection) : base(wrappedConnection)
        {
            _currentKeyspaceName = GetFirstAvailableKeyspace(wrappedConnection);
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

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) =>
            new CassandraMetadataTable(schema, tableName, this);

        /// <summary>
        ///     Will check for a predefined keyspace and table to see if there is a lock.
        ///     Otherwise, always returns true, because the lock is granted at table level.
        /// </summary>
        public override bool TryAcquireApplicationLock()
        {
            string clusterLockKeyspaceName = "cluster_lock";
            string clusterLockTableName = "lock";

            if (Configuration.ConfigurationFileExists())
            {
                var clusterLock = JSON.Parse(Configuration.GetConfiguration()).Linq
                                      .SingleOrDefault(x => x.Key.Equals("clusterLock", StringComparison.OrdinalIgnoreCase)).Value?.Linq
                                      .ToDictionary(x => x.Key, x => x.Value.Value, StringComparer.OrdinalIgnoreCase);

                clusterLockKeyspaceName = clusterLock!.GetValue("defaultClusterLockKeyspace", clusterLockKeyspaceName)!;
                clusterLockTableName = clusterLock!.GetValue("defaultClusterLockTable", clusterLockTableName)!;
            }

            try
            {
                return WrappedConnection.QueryForLong($"select count(locked) from {clusterLockKeyspaceName}.{clusterLockTableName}") == 0;
            }
            catch (EvolveException ex)
                when (ex?.InnerException?.GetType().ToString() == "Cassandra.InvalidQueryException")
            {
                //These error messages are very specific to Cassandra's drivers and could change
                return ex.Message.StartsWith($"keyspace {clusterLockKeyspaceName} does not exist", StringComparison.OrdinalIgnoreCase)
                    || ex.Message.StartsWith($"unconfigured table {clusterLockTableName}", StringComparison.OrdinalIgnoreCase)
                    || ex.Message.StartsWith($"table {clusterLockTableName} does not exist", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        ///     Returns always true, because the lock is released at table level.
        /// </summary>
        public override bool ReleaseApplicationLock() => true;

        public override SqlStatementBuilderBase SqlStatementBuilder { get; } = new CqlStatementBuilder();
    }
}
