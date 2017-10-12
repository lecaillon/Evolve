using System.Data;
using Evolve.Driver;
using Xunit;

namespace Evolve.Test.Driver
{
    [Collection("Database collection")]
    public class NetDriverTest
    {
        private readonly DatabaseFixture _fixture;

        public NetDriverTest(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

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
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_fixture.Pg.HostPort};Database={_fixture.Pg.DbName};User Id={_fixture.Pg.DbUser};Password={_fixture.Pg.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "CoreMySqlDriverForNet_works")]
        public void CoreMySqlDriverForNet_works()
        {

            var driver = new CoreMySqlDataDriverForNet(TestContext.DriverResourcesDepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_fixture.MySql.HostPort};Database={_fixture.MySql.DbName};Uid={_fixture.MySql.DbUser};Pwd={_fixture.MySql.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "SqlClientDriver_works")]
        public void SqlClientDriver_works()
        {
            var driver = new SqlClientDriver();
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Database=master;User Id={_fixture.MsSql.DbUser};Password={_fixture.MsSql.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        #endregion
    }
}
