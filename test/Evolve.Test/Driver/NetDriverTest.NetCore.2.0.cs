using System.Data;
using Evolve.Driver;
using Xunit;

namespace Evolve.Test.Driver
{
    public partial class NetDriverTest
    {
        [Trait("Category", "Standalone")]
        [Fact(DisplayName = "CoreMicrosoftDataSqliteDriverForNet_NET_Core_2_0_works")]
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
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_pgFixture.HostPort};Database={_pgFixture.DbName};User Id={_pgFixture.DbUser};Password={_pgFixture.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "CoreMySqlDriverForNet_NET_Core_2_0_works")]
        public void CoreMySqlDriverForNet_NET_Core_2_0_works()
        {
            var driver = new CoreMySqlDataDriverForNet(TestContext.NetCore20DepsFile, TestContext.NugetPackageFolder);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_mySqlfixture.HostPort};Database={_mySqlfixture.DbName};Uid={_mySqlfixture.DbUser};Pwd={_mySqlfixture.DbPwd};SslMode=none;");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }
    }
}
