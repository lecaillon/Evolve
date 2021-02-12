using System;
using System.Collections.Generic;
using System.Linq;
using Evolve.Connection;
using SimpleJSON;

namespace Evolve.Dialect.Cassandra
{
    internal sealed class CassandraKeyspace : Schema
    {
        private readonly ReplicationStrategy _replicationStrategy;

        public CassandraKeyspace(string keyspaceName, ReplicationStrategy replicationStrategy, WrappedConnection wrappedConnection)
            : base(keyspaceName, wrappedConnection)
        {
            _replicationStrategy = replicationStrategy ?? throw new ArgumentNullException(nameof(replicationStrategy));
        }

        public override bool Create()
        {
            var cql = $"create keyspace if not exists {Name} with replication = {_replicationStrategy.ToCql()}";
            _wrappedConnection.ExecuteNonQuery(cql);

            return true;
        }

        public override bool Drop()
        {
            _wrappedConnection.ExecuteNonQuery($"drop keyspace {Name}");
            return true;
        }

        public override bool Erase()
        {
            var functions = _wrappedConnection.QueryForListOfString($"select function_name from system_schema.functions where keyspace_name = '{Name}'");
            foreach (var function in functions)
            {
                _wrappedConnection.ExecuteNonQuery($"drop function if exists {Name}.{function}");
            }

            var triggers = _wrappedConnection.QueryForList($"select table_name, trigger_name from system_schema.triggers where keyspace_name = '{Name}'", r => new { Table = r.GetString(0), Name = r.GetString(1) });
            foreach (var trigger in triggers)
            {
                _wrappedConnection.ExecuteNonQuery($"drop trigger if exists {trigger.Name} on {Name}.{trigger.Table}");
            }

            var views = _wrappedConnection.QueryForListOfString($"select view_name from system_schema.views where keyspace_name = '{Name}'");
            foreach (var view in views)
            {
                _wrappedConnection.ExecuteNonQuery($"drop materialized view if exists {Name}.{view}");
            }

            var indexes = _wrappedConnection.QueryForListOfString($"select index_name from system_schema.indexes where keyspace_name = '{Name}'");
            foreach (var index in indexes)
            {
                _wrappedConnection.ExecuteNonQuery($"drop index if exists {Name}.{index}");
            }

            var tables = _wrappedConnection.QueryForListOfString($"select table_name from system_schema.tables where keyspace_name = '{Name}'");
            foreach (var table in tables)
            {
                _wrappedConnection.ExecuteNonQuery($"drop table if exists {Name}.{table}");
            }

            return true;
        }

        public override bool IsEmpty() =>
            _wrappedConnection.QueryForLong($"select count(table_name) from system_schema.tables where keyspace_name = '{Name}'") == 0
            && _wrappedConnection.QueryForLong($"select count(index_name) from system_schema.indexes where keyspace_name = '{Name}'") == 0
            && _wrappedConnection.QueryForLong($"select count(trigger_name) from system_schema.triggers where keyspace_name = '{Name}'") == 0
            && _wrappedConnection.QueryForLong($"select count(function_name) from system_schema.functions where keyspace_name = '{Name}'") == 0
            && _wrappedConnection.QueryForLong($"select count(view_name) from system_schema.views where keyspace_name = '{Name}'") == 0;

        public override bool IsExists() =>
            _wrappedConnection.QueryForLong($"select count(keyspace_name) from system_schema.keyspaces where keyspace_name = '{Name}'") == 1;

        public static CassandraKeyspace Retreive(string keyspaceName, WrappedConnection wrappedConnection)
        {
            var replication = wrappedConnection.Query<SortedDictionary<string, string>>($"select replication from system_schema.keyspaces where keyspace_name = '{keyspaceName}'");

            if (replication == null || !replication.Any())
                return new CassandraKeyspace(keyspaceName, GetReplicationStrategyFromConfiguration(keyspaceName), wrappedConnection);

            return new CassandraKeyspace(keyspaceName, ReplicationStrategy.FromSortedDictionary(replication), wrappedConnection);
        }

        private static ReplicationStrategy GetReplicationStrategyFromConfiguration(string keyspaceName)
        {
            if (Configuration.ConfigurationFileExists())
            {
                var keyspaces = JSON.Parse(Configuration.GetConfiguration()).Linq
                                    .SingleOrDefault(x => x.Key.Equals("keyspaces", StringComparison.OrdinalIgnoreCase)).Value?.Linq
                                    .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

#pragma warning disable CS8620
                if (keyspaces?.GetValue(keyspaceName) != null)
                {
                    return ReplicationStrategy.FromSortedDictionary(new SortedDictionary<string, string>((keyspaces[keyspaceName].Linq.ToDictionary(x => x.Key, x => x.Value.Value))));
                }
                else if (keyspaces?.GetValue(Configuration.DefaultKeyspaceKey) != null)
#pragma warning restore CS8620
                {
                    return ReplicationStrategy.FromSortedDictionary(new SortedDictionary<string, string>((keyspaces[Configuration.DefaultKeyspaceKey].Linq.ToDictionary(x => x.Key, x => x.Value.Value))));
                }
                else
                {
                    return CreateSimpleStrategy(1); //Default if the the keyspace name is not present and there is no default
                }
            }
            else
            {
                return CreateSimpleStrategy(1); //Default if the file is not present
            }
        }

        public abstract class ReplicationStrategy
        {
            public abstract string ToCql();

            internal static ReplicationStrategy FromSortedDictionary(SortedDictionary<string, string> properties)
            {
                var type = properties["class"];
                switch (type)
                {
                    case "LocalStrategy":
                    case "org.apache.cassandra.locator.LocalStrategy":
                        return CreateLocalStrategy();
                    case "SimpleStrategy":
                    case "org.apache.cassandra.locator.SimpleStrategy":
                        return CreateSimpleStrategy(int.Parse(properties["replication_factor"]));
                    case "NetworkTopologyStrategy":
                    case "org.apache.cassandra.locator.NetworkTopologyStrategy":
                        return CreateNetworkTopologyStrategy(
                            properties
                                .Where(i => i.Key != "class")
                                .Select(i =>
                                {
                                    return new DataCenterReplicationFactor(i.Key, int.Parse(i.Value));
                                }).ToArray());
                    default:
                        throw new NotSupportedException($"The replication type {type} is not supported");
                }
            }
        }

        internal static ReplicationStrategy CreateLocalStrategy() => new LocalStrategy();

        public sealed class LocalStrategy : ReplicationStrategy
        {
            internal LocalStrategy() { }

            public override string ToCql() => "{ 'class' : 'LocalStrategy' }";
        }

        public static ReplicationStrategy CreateSimpleStrategy(int replicationFactor) => new SimpleStrategy(replicationFactor);

        public sealed class SimpleStrategy : ReplicationStrategy
        {
            public int ReplicationFactor { get; }

            internal SimpleStrategy(int replicationFactor)
            {
                this.ReplicationFactor = replicationFactor;
            }

            public override string ToCql() =>
                "{ " +
                    "'class' : 'SimpleStrategy', " +
                    $"'replication_factor' : {this.ReplicationFactor}" +
                "}";
        }

        public static ReplicationStrategy CreateNetworkTopologyStrategy(params DataCenterReplicationFactor[] dataCentersReplicationFactors) =>
            new NetworkTopologyStrategy(dataCentersReplicationFactors);

        public sealed class NetworkTopologyStrategy : ReplicationStrategy
        {
            public IEnumerable<DataCenterReplicationFactor> DataCentersReplicationFactors { get; }

            internal NetworkTopologyStrategy(params DataCenterReplicationFactor[] dataCentersReplicationFactors)
            {
                this.DataCentersReplicationFactors = dataCentersReplicationFactors;
            }

            public override string ToCql() =>
                "{ " +
                    "'class' : 'NetworkTopologyStrategy', " +
                    string.Join(", ", this.DataCentersReplicationFactors.Select(i => $"'{i.DataCenter}' : {i.ReplicationFactor}").ToArray()) +
                "}";
        }

        public class DataCenterReplicationFactor
        {
            public string DataCenter { get; }
            public int ReplicationFactor { get; }

            public DataCenterReplicationFactor(string dataCenter, int replicationFactor)
            {
                this.DataCenter = dataCenter;
                this.ReplicationFactor = replicationFactor;
            }
        }
    }
}
