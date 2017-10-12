using System.Runtime.InteropServices;

namespace Evolve.Test.Utilities
{
    public class PostgreSqlDockerContainer : IDockerContainer
    {
        private DockerContainer _container;

        public string Id => _container.Id;
        public string ExposedPort => "5432";
        public string HostPort => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "3306" : "3307"; //  AppVeyor: 5432 Travis CI: 5433
        public string DbName => "my_database";
        public string DbPwd => "Password12!"; // AppVeyor
        public string DbUser => "postgres";

        public bool Start()
        {
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "postgres",
                Tag = "alpine",
                Name = "postgres-evolve",
                Env = new[] { $"POSTGRES_PASSWORD={DbPwd}", $"POSTGRES_DB={DbName}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort
            }).Build();

            return _container.Start();
        }
        public void Remove() => _container.Remove();
        public bool Stop() => _container.Stop();
        public void Dispose() => _container.Dispose();
    }
}
