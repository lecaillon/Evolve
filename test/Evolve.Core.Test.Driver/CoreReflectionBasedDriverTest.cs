using System.Data;
using System.Threading;
using Evolve.Driver;
using Xunit;

namespace Evolve.Core.Test.Driver
{
    [Collection("Database collection")]
    public class CoreReflectionBasedDriverTest
    {
        private readonly DatabaseFixture _fixture;

        public CoreReflectionBasedDriverTest(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

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
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_fixture.Pg.HostPort};Database={_fixture.Pg.DbName};User Id={_fixture.Pg.DbUser};Password={_fixture.Pg.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "MySqlDriver_works")]
        public void MySqlDriver_works()
        {

            var driver = new CoreMySqlDataDriver(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_fixture.MySql.HostPort};Database={_fixture.MySql.DbName};Uid={_fixture.MySql.DbUser};Pwd={_fixture.MySql.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "SqlClientDriver_works")]
        public void SqlClientDriver_works()
        {
            Thread.Sleep(60000);
            var driver = new CoreSqlClientDriver(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Database=master;User Id={_fixture.MsSql.DbUser};Password={_fixture.MsSql.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }
    }
}
