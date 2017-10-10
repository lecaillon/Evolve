using System;
using System.Data.SqlClient;
using System.Management.Automation;
using System.Threading;
using Xunit;

namespace Evolve.IntegrationTest.SQLServer
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
#if DEBUG
            using (var ps = PowerShell.Create())
            {
                ps.Commands.AddScript($"docker run --name '{TestContext.ContainerName}' -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD={TestContext.DbPwd}' --publish={TestContext.ContainerPort}:{TestContext.ContainerPort} -d {TestContext.ImageName}");
                ps.Invoke();
            }

            Thread.Sleep(20000);
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

        public void CreateTestDatabase(string name)
        {
            var cnn = new SqlConnection($"Server=127.0.0.1;Database=master;User Id={TestContext.DbUser};Password={TestContext.DbPwd};");
            cnn.Open();

            using (var cmd = cnn.CreateCommand())
            {
                cmd.CommandText = $"CREATE DATABASE {name};";
                cmd.ExecuteNonQuery();
            }

            cnn.Close();
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
