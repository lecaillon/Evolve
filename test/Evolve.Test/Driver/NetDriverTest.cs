using System.Data;
using Evolve.Driver;
using Xunit;

namespace Evolve.Test.Driver
{
    public class NetDriverTest
    {
        [Fact(DisplayName = "Load_ConnectionType_from_an_already_loaded_assembly")]
        public void Load_ConnectionType_from_an_already_loaded_assembly()
        {
            var driver = new SystemDataSQLiteDriver();
            var cnn = driver.CreateConnection("Data Source=:memory:");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        #region CoreDriverConnectionProviderForNet

        [Fact(DisplayName = "CoreMicrosoftDataSqliteDriverForNet_works")]
        public void CoreMicrosoftDataSqliteDriverForNet_works()
        {
            var driver = new CoreMicrosoftDataSqliteDriverForNet(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection("Data Source=:memory:");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "CoreNpgsqlDriverForNet_works")]
        public void CoreNpgsqlDriverForNet_works()
        {

            var driver = new CoreNpgsqlDriverForNet(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port=5432;Database={TestContext.DbName};User Id=postgres;Password={TestContext.DbPassword};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "CoreMySqlDriverForNet_works")]
        public void CoreMySqlDriverForNet_works()
        {

            var driver = new CoreMySqlDataDriverForNet(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port=3306;Database={TestContext.DbName};Uid=root;Pwd={TestContext.DbPassword};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "SqlClientDriver_works")]
        public void SqlClientDriver_works()
        {
            var driver = new SqlClientDriver();
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Database=master;User Id=sa;Password={TestContext.DbPassword};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        #endregion
    }
}
