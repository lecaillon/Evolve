using System;
using System.Runtime.InteropServices;

namespace Evolve.Test.Utilities
{
    public class MySQLDockerContainer
    {
        private DockerContainer _container;

        public string Id => _container.Id;
        public string ExposedPort => "3306";
        public string HostPort => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "3306" : "3307"; //  AppVeyor: 3306 Travis CI: 3307
        public string DbName => "my_database";
        public string DbPwd => "Password12!"; // AppVeyor
        public string DbUser => "root";
        public TimeSpan DelayAfterStartup => TimeSpan.FromSeconds(10);

        public bool Start(bool fromScratch = false)
        {
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "mariadb",
                Tag = "latest",
                Name = "mariadb-evolve",
                Env = new[] { $"MYSQL_ROOT_PASSWORD={DbPwd}", $"MYSQL_DATABASE={DbName}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort,
                DelayAfterStartup = DelayAfterStartup,
                RemovePreviousContainer = fromScratch
            }).Build();

            return _container.Start();
        }
        public void Remove() => _container.Remove();
        public bool Stop() => _container.Stop();
        public void Dispose() => _container.Dispose();
    }
}
