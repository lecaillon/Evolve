using System.Data.SqlClient;
using System.Management.Automation;
using System.Threading;

namespace Evolve.IntegrationTest.SQLServer
{
    public static class TestUtil
    {
        public static void RunContainer()
        {
#if DEBUG
            using (var ps = PowerShell.Create())
            {
                ps.Commands.AddScript($"docker run --name {TestContext.ContainerName} -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD={TestContext.DbPwd}' --publish={TestContext.ContainerPort}:{TestContext.ContainerPort} -d {TestContext.ImageName}");
                ps.Invoke();
            }

            Thread.Sleep(30000);
#endif
        }

        public static void RemoveContainer()
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

        public static void CreateTestDatabase()
        {
            var cnn = new SqlConnection($"Server=127.0.0.1;Database=master;User Id={TestContext.DbUser};Password={TestContext.DbPwd};");
            cnn.Open();

            using (var cmd = cnn.CreateCommand())
            {
                cmd.CommandText = $"CREATE DATABASE {TestContext.DbName};";
                cmd.ExecuteNonQuery();
            }

            cnn.Close();
        }
    }
}
