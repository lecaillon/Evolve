using System.Data;
using Evolve.Driver;
using Xunit;

namespace Evolve.Test.Driver
{
    public partial class NetDriverTest
    {
        [Fact(DisplayName = "CoreMicrosoftDataSqliteDriverForNet_NET_Core_2_0_works", Skip = "Does not work in NetStandard 2.0")]
        public void CoreMicrosoftDataSqliteDriverForNet_NET_Core_2_0_works()
        {
            var driver = new CoreMicrosoftDataSqliteDriverForNet(TestContext.NetCore20DepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection("Data Source=:memory:");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "CoreNpgsqlDriverForNet_NET_Core_2_0_works")]
        public void CoreNpgsqlDriverForNet_NET_Core_2_0_works()
        {
            var driver = new CoreNpgsqlDriverForNet(TestContext.NetCore20DepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_fixture.Pg.HostPort};Database={_fixture.Pg.DbName};User Id={_fixture.Pg.DbUser};Password={_fixture.Pg.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "CoreMySqlDriverForNet_NET_Core_2_0_works")]
        public void CoreMySqlDriverForNet_NET_Core_2_0_works()
        {
            var driver = new CoreMySqlDataDriverForNet(TestContext.NetCore20DepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_fixture.MySql.HostPort};Database={_fixture.MySql.DbName};Uid={_fixture.MySql.DbUser};Pwd={_fixture.MySql.DbPwd};SslMode=none;");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }
    }
}
