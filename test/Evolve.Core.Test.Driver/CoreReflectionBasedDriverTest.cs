using System;
using System.Data;
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

        [Fact(DisplayName = "TEST")]
        public void TEST()
        {
            Assert.True(TestContext.DriverResourcesDepsFile == "PSG");
        }


        [Fact(DisplayName = "SqlClientDriver_works", Skip = "Temp skipped")]
        public void SqlClientDriver_works()
        {
            var driver = new CoreSqlClientDriver(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection("Server=127.0.0.1;Database=master;User Id=sa;Password=Password12!;");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }
    }
}
