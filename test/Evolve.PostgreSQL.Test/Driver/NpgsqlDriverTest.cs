using System;
using Evolve.Driver;
using Npgsql;
using Xunit;

namespace Evolve.PostgreSQL.Test
{
    public class NpgsqlDriverTest
    {
        [Fact]
        public void When_NpgsqlDriver_is_loaded_reference_is_returned()
        {
            var cnx = new NpgsqlConnection();
            var driver = new NpgsqlDriver();
            Assert.NotNull(driver.ConnectionType);
        }
    }
}
