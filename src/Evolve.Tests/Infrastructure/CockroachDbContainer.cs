using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;

namespace EvolveDb.Tests.Infrastructure
{
    public class CockroachDBContainer : IDbContainer
    {
        public const string ExposedPort = "26257";
        public const string HostPort = "26257";
        public const string DbName = "defaultdb";

        private DockerContainer _container;

        public string Id => _container?.Id;
        public string CnxStr => $"Host=localhost;Username=root;Port={HostPort};Database={DbName};";
        public int TimeOutInSec => 8;

        public async Task<bool> Start(bool fromScratch = false)
        {
            _container = await new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "cockroachdb/cockroach",
                Tag = "latest",
                Name = "cockroachdb-evolve",
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort,
                RemovePreviousContainer = fromScratch,
                Cmd = new[] { "start-single-node", "--insecure" }
            }).Build();

            return await _container.Start();
        }

        public DbConnection CreateDbConnection() => new NpgsqlConnection(CnxStr);
    }
}
