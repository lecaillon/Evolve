using System.Data;
using System.Threading;
using Evolve.Driver;
using Xunit;

namespace Evolve.Core2.Test.Driver
{
    [Collection("Database collection")]
    public class CoreReflectionBasedDriverTest
    {
        private readonly DatabaseFixture _fixture;

        public CoreReflectionBasedDriverTest(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "MicrosoftDataSqliteDriver_NET_Core_2_0_works")]
        public void MicrosoftDataSqliteDriver_NET_Core_2_0_works()
        {
            var driver = new CoreMicrosoftDataSqliteDriver(TestContext.NetCore20DepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection("Data Source=:memory:");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "NpgsqlDriver_NET_Core_2_0_works")]
        public void NpgsqlDriver_NET_Core_2_0_works()
        {
            
            var driver = new CoreNpgsqlDriver(TestContext.NetCore20DepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_fixture.Pg.HostPort};Database={_fixture.Pg.DbName};User Id={_fixture.Pg.DbUser};Password={_fixture.Pg.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "MySqlDriver_NET_Core_2_0_works")]
        public void MySqlDriver_NET_Core_2_0_works()
        {

            var driver = new CoreMySqlDataDriver(TestContext.NetCore20DepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_fixture.MySql.HostPort};Database={_fixture.MySql.DbName};Uid={_fixture.MySql.DbUser};Pwd={_fixture.MySql.DbPwd};SslMode=none;");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "SqlClientDriver_NET_Core_2_0_works")]
        public void SqlClientDriver_NET_Core_2_0_works()
        {
            Thread.Sleep(60000);
            var driver = new CoreSqlClientDriver(TestContext.NetCore20DepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Database=master;User Id={_fixture.MsSql.DbUser};Password={_fixture.MsSql.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }
    }
}
