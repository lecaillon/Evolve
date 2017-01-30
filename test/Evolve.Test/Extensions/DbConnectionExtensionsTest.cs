using System.Data;
using Evolve.Extensions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Evolve.Test.Extensions
{
    public class DbConnectionExtensionsTest
    {
        [Fact(DisplayName = "QueryForLong works")]
        public void QueryForLong_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                Assert.Equal(1L, DbConnectionExtensions.QueryForLong(connection, "SELECT 1;"));
                Assert.True(connection.State == ConnectionState.Closed);
            }
        }
    }
}
