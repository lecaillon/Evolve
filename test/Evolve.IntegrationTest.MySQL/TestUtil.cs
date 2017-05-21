using System.Management.Automation;
using System.Threading;

namespace Evolve.IntegrationTest.MySQL
{
    public static class TestUtil
    {
        public static void RunContainer()
        {
#if DEBUG
            using (var ps = PowerShell.Create())
            {
                ps.Commands.AddScript($"docker run --name {TestContext.ContainerName} --publish={TestContext.ContainerPort}:{TestContext.ContainerPort} -e MYSQL_ROOT_PASSWORD={TestContext.DbPwd} -e MYSQL_DATABASE={TestContext.DbName} -d {TestContext.ImageName}");
                ps.Invoke();
            }

            Thread.Sleep(20000);
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
    }
}
