using System;

namespace Evolve.Test.Utilities
{
    public class CassandraDockerContainer
    {
        private DockerContainer _container;

        public string Id => _container.Id;
        public string ExposedPort => "9042";
        public string HostPort => "9042";
        public string ClusterName => "evolve";
        public string DataCenter => "dc1";
        public TimeSpan DelayAfterStartup => TimeSpan.FromMinutes(1);

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
                DelayAfterStartup = DelayAfterStartup,
                RemovePreviousContainer = fromScratch
            }).Build();

            return _container.Start();
        }
        public void Remove() => _container.Remove();
        public bool Stop() => _container.Stop();
        public void Dispose() => _container?.Dispose();
    }
}
