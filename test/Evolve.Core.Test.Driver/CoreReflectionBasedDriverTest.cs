using System.Data;
using System.Threading;
using Evolve.Driver;
using Xunit;

namespace Evolve.Core.Test.Driver
{
    public class CoreReflectionBasedDriverTest
    {
        [Fact(DisplayName = "MicrosoftDataSqliteDriver_works")]
        public void MicrosoftDataSqliteDriver_works()
        {
            var driver = new CoreMicrosoftDataSqliteDriver(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection("Data Source=:memory:");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "NpgsqlDriver_works")]
        public void NpgsqlDriver_works()
        {
            
            var driver = new CoreNpgsqlDriver(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port=5432;Database=my_database;User Id=postgres;Password={TestContext.PgPassword};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "MySqlDriver_works")]
        public void MySqlDriver_works()
        {

            var driver = new CoreMySqlDataDriver(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port=3306;Database=my_database;Uid=root;Pwd={TestContext.MySqlPassword};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "SqlClientDriver_works")]
        public void SqlClientDriver_works()
        {
            Thread.Sleep(60000);
            var driver = new CoreSqlClientDriver(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection("Server=127.0.0.1;Database=master;User Id=sa;Password=Password12!;");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }
    }
}
