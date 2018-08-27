using System.Data;
using Evolve.Driver;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.Test.Driver
{
    [Collection("Database collection")]
    public partial class NetDriverTest
    {
        private readonly MySQLFixture _mySqlfixture;
        private readonly PostgreSqlFixture _pgFixture;
        private readonly SQLServerFixture _sqlServerFixture;
        private readonly CassandraFixture _cassandraFixture;

        public NetDriverTest(MySQLFixture mySqlfixture, PostgreSqlFixture pgFixture, SQLServerFixture sqlServerFixture, CassandraFixture cassandraFixture)
        {
            _mySqlfixture = mySqlfixture;
            _pgFixture = pgFixture;
            _sqlServerFixture = sqlServerFixture;
            _cassandraFixture = cassandraFixture;

            if (!TestContext.Travis && !TestContext.AppVeyor)
            { // AppVeyor and Windows 2016 does not support linux docker images
                _mySqlfixture.Start();
                _pgFixture.Start();
                _sqlServerFixture.Start();
                _cassandraFixture.Start();
            }
        }

        [Fact(DisplayName = "Load_ConnectionType_from_an_already_loaded_assembly")]
        public void Load_ConnectionType_from_an_already_loaded_assembly()
        {
            var driver = new SystemDataSQLiteDriver();
            var cnn = driver.CreateConnection("Data Source=:memory:");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }

        [Fact(DisplayName = "SqlClientDriver_works")]
        public void SqlClientDriver_works()
        {
            var driver = new SqlClientDriver();
            var cnn = driver.CreateConnection($"Server=127.0.0.1;Database=master;User Id={_sqlServerFixture.DbUser};Password={_sqlServerFixture.DbPwd};");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }
    }
}
