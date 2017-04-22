using System.Data;
using Evolve.Driver;
using Xunit;

namespace Evolve.Test.Driver
{
    public class SystemDataSQLiteDriverTest
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
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port=5432;Database=my_database;User Id=postgres;Password={TestContext.PgPassword};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "SqlClientDriver_works")]
        public void SqlClientDriver_works()
        {
            var driver = new SqlClientDriver();
            var cnn = driver.CreateConnection("Server=127.0.0.1;Database=master;User Id=sa;Password=Password12!;");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        #endregion
    }
}
