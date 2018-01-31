using System;
using System.Runtime.InteropServices;

namespace Evolve.Test.Utilities
{
    public class PostgreSqlDockerContainer
    {
        private DockerContainer _container;

        public string Id => _container.Id;
        public string ExposedPort => "5432";
        public string HostPort => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "5432" : "5433"; //  AppVeyor: 5432 Travis CI: 5433
        public string DbName => "my_database";
        public string DbPwd => "Password12!"; // AppVeyor
        public string DbUser => "postgres";
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
