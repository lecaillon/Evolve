using System.Data;
using Evolve.Extensions;
using Microsoft.Data.Sqlite;
using Xunit;
using System.Collections.Generic;

namespace Evolve.Test.Extensions
{
    public class DbConnectionExtensionsTest
    {
        [Fact(DisplayName = "QueryForLong_works")]
        public void QueryForLong_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                Assert.Equal(1L, DbConnectionExtensions.QueryForLong(connection, "SELECT 1;"));
                Assert.True(connection.State == ConnectionState.Closed);
            }
        }

        [Fact(DisplayName = "QueryForString_works")]
        public void QueryForString_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                Assert.Equal("azerty", DbConnectionExtensions.QueryForString(connection, "SELECT 'azerty';"));
                Assert.True(connection.State == ConnectionState.Closed);
            }
        }

        [Fact(DisplayName = "QueryForListOfString_works")]
        public void QueryForListOfString_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                Assert.Equal(new List<string> { "azerty", "qwerty" }, DbConnectionExtensions.QueryForListOfString(connection, "SELECT 'azerty' UNION SELECT 'qwerty';"));
                Assert.True(connection.State == ConnectionState.Closed);
            }
        }
    }
}
