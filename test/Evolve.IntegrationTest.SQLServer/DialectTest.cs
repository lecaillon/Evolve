using System;
using System.Data;
using System.Data.SqlClient;
using Xunit;

namespace Evolve.IntegrationTest.SQLServer
{
    public class DialectTest : IDisposable
    {
        [Fact(DisplayName = "Run_all_SQLServer_integration_tests_work")]
        public void Run_all_SQLServer_integration_tests_work()
        {
            // Open a connection to the PostgreSQL database
            var cnn = new SqlConnection($"Server=127.0.0.1;Database=master;User Id={TestContext.DbUser};Password={TestContext.DbPwd};");
            cnn.Open();
            Assert.True(cnn.State == ConnectionState.Open, "Cannot open a connection to the database.");

        }

        /// <summary>
        ///     Start PostgreSQL server.
        /// </summary>
        public DialectTest()
        {
            TestUtil.RunContainer();
        }

        /// <summary>
        ///     Stop PostgreSQL server and remove container.
        /// </summary>
        public void Dispose()
        {
            TestUtil.RemoveContainer();
        }
    }
}
