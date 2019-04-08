using System.Data;
using Npgsql;

namespace Evolve.Tests.Infrastructure
{
    public class CockroachDbContainer : IDbContainer
    {
        public const string ExposedPort = "26257";
        public const string HostPort = "26257";
        public const string DbName = "defaultdb";

        private DockerContainer _container;

        public string Id => _container?.Id;
        public string CnxStr => $"Host=localhost;Username=root;Port={HostPort};Database={DbName};";
        public int TimeOutInSec => 8;

        public bool Start(bool fromScratch = false)
        {
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "cockroachdb/cockroach",
                Tag = "v2.1.6",
                Name = "cockroachdb-evolve",
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort,
                RemovePreviousContainer = fromScratch,
                Cmd = new[] { "start", "--insecure" }
            }).Build();

            return _container.Start();
        }

        public IDbConnection CreateDbConnection() => new NpgsqlConnection(CnxStr);

        public void Dispose() => _container?.Dispose();
    }
}
