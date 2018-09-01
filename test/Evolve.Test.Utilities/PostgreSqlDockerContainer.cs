using System;

namespace Evolve.Test.Utilities
{
    public class PostgreSqlDockerContainer
    {
        private DockerContainer _container;

        public string Id => _container.Id;
        public string ExposedPort => "5432";
        public string HostPort => Environment.GetEnvironmentVariable("PG_PORT") ?? "5432";
        public string DbName => "my_database";
        public string DbPwd => "Password12!"; // AppVeyor
        public string DbUser => "postgres";
        public string CnxStr => $"Server=127.0.0.1;Port={HostPort};Database={DbName};User Id={DbUser};Password={DbPwd};";
        public TimeSpan DelayAfterStartup => TimeSpan.FromSeconds(5);

        public bool Start(bool fromScratch = false)
        {
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "postgres",
                Tag = "alpine",
                Name = "postgres-evolve",
                Env = new[] { $"POSTGRES_PASSWORD={DbPwd}", $"POSTGRES_DB={DbName}" },
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
