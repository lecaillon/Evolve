using System;
using System.Management.Automation;
using System.Threading;
using Xunit;

namespace Evolve.IntegrationTest.PostgreSQL
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
#if DEBUG
            using (var ps = PowerShell.Create())
            {
                ps.Commands.AddScript($"docker run --name {TestContext.ContainerName} --publish={TestContext.ContainerPort}:{TestContext.ContainerPort} -e POSTGRES_PASSWORD={TestContext.DbPwd} -e POSTGRES_DB={TestContext.DbName} -d {TestContext.ImageName}");
                ps.Invoke();
            }

            Thread.Sleep(5000);
#endif
        }

        public void Dispose()
        {
#if DEBUG
            using (var ps = PowerShell.Create())
            {
                ps.Commands.AddScript($"docker stop '{TestContext.ContainerName}'");
                ps.Commands.AddScript($"docker rm '{TestContext.ContainerName}'");
                ps.Invoke();
            }
#endif
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
