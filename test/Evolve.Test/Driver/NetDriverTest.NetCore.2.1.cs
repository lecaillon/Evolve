using System.Data;
using Evolve.Driver;
using Xunit;

namespace Evolve.Test.Driver
{
    public partial class NetDriverTest
    {
        [Trait("Category", "Standalone")]
        [Fact(DisplayName = "CoreMicrosoftDataSqliteDriverForNet_NET_Core_2_1_works")]
        public void CoreMicrosoftDataSqliteDriverForNet_NET_Core_2_1_works()
        {
            var driver = new CoreMicrosoftDataSqliteDriverForNet(TestContext.NetCore21DepsFile, TestContext.NugetPackageFolder, msBuildExtensionsPath: null);
            var cnn = driver.CreateConnection("Data Source=:memory:");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "CoreNpgsqlDriverForNet_NET_Core_2_1_works")]
        public void CoreNpgsqlDriverForNet_NET_Core_2_1_works()
        {
            var driver = new CoreNpgsqlDriverForNet(TestContext.NetCore21DepsFile, TestContext.NugetPackageFolder, msBuildExtensionsPath: null);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_pgFixture.HostPort};Database={_pgFixture.DbName};User Id={_pgFixture.DbUser};Password={_pgFixture.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "CoreMySqlDriverForNet_NET_Core_2_1_works")]
        public void CoreMySqlDriverForNet_NET_Core_2_1_works()
        {
            var driver = new CoreMySqlDataDriverForNet(TestContext.NetCore21DepsFile, TestContext.NugetPackageFolder, msBuildExtensionsPath: null);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_mySqlfixture.HostPort};Database={_mySqlfixture.DbName};Uid={_mySqlfixture.DbUser};Pwd={_mySqlfixture.DbPwd};SslMode=none;");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "CoreMySqlConnectorDriverForNet_NET_Core_2_1_works")]
        public void CoreMySqlConnectorDriverForNet_NET_Core_2_1_works()
        {

            var driver = new CoreMySqlConnectorDriverForNet(TestContext.NetCore21DepsFile, TestContext.NugetPackageFolder, msBuildExtensionsPath: null);
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Port={_mySqlfixture.HostPort};Database={_mySqlfixture.DbName};Uid={_mySqlfixture.DbUser};Pwd={_mySqlfixture.DbPwd};SslMode=none;");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "CassandraDriver_NET_Core_2_1_works")]
        public void CassandraDriver_NET_Core_2_1_works()
        {
            if (!TestContext.AppVeyor)
            {
                var driver = new CoreCassandraDriverForNet(TestContext.NetCore21DepsFile, TestContext.NugetPackageFolder, msBuildExtensionsPath: null);
                var cnn = driver.CreateConnection($"Contact Points=127.0.0.1;Port={_cassandraFixture.Cassandra.HostPort};Cluster Name={_cassandraFixture.Cassandra.ClusterName}");
                cnn.Open();

                Assert.True(cnn.State == ConnectionState.Open);
            }
        }
    }
}
