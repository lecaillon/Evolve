﻿using System.Data;
using Cassandra.Data;

namespace Evolve.Tests.Infrastructure
{
    public class CassandraContainer : IDbContainer
    {
        public const string ExposedPort = "9042";
        public const string HostPort = "9042";
        public const string ClusterName = "evolve";
        public const string DataCenter = "dc1";
        public const string DbUser = "postgres";

        private DockerContainer _container;

        public string Id => _container?.Id;
        public string CnxStr => $"Contact Points=127.0.0.1;Port={HostPort};Cluster Name={ClusterName}";
        public int TimeOutInSec => 45;

        public bool Start(bool fromScratch = false)
        {
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "cassandra",
                Tag = "latest",
                Name = "cassandra-evolve",
                Env = new[] { $"CASSANDRA_CLUSTER_NAME={ClusterName}", $"CASSANDRA_DC={DataCenter}", "CASSANDRA_RACK=rack1" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort,
                RemovePreviousContainer = fromScratch
            }).Build();

            return _container.Start();
        }

        public IDbConnection CreateDbConnection() => new CqlConnection(CnxStr);

        public void Dispose() => _container?.Dispose();
    }
}
