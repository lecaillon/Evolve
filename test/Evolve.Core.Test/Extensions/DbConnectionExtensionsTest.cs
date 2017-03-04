using System.Data;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Evolve.Dialect;

namespace Evolve.Test.Core.Extensions
{
    public class WrappedConnectionExtensionsTest
    {
        [Fact(DisplayName = "GetDatabaseServerType_is_sqlite")]
        public void GetDatabaseServerType_is_sqlite()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                Assert.Equal(DBMS.SQLite, WrappedConnectionEx.GetDatabaseServerType(connection));
            }
        }

        [Fact(DisplayName = "QueryForLong_works")]
        public void QueryForLong_works()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                Assert.Equal(1L, WrappedConnectionEx.QueryForLong(connection, "SELECT 1;"));
                Assert.True(connection.DbConnection.State == ConnectionState.Closed);
            }
        }

        [Fact(DisplayName = "QueryForString_works")]
        public void QueryForString_works()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                Assert.Equal("azerty", WrappedConnectionEx.QueryForString(connection, "SELECT 'azerty';"));
                Assert.True(connection.DbConnection.State == ConnectionState.Closed);
            }
        }

        [Fact(DisplayName = "QueryForListOfString_works")]
        public void QueryForListOfString_works()
        {
            var expected = new List<string> { "azerty", "qwerty" };
            string sql = "SELECT 'azerty' UNION SELECT 'qwerty';";

            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                Assert.Equal(expected, WrappedConnectionEx.QueryForListOfString(connection, sql));
                Assert.True(connection.DbConnection.State == ConnectionState.Closed);
            }
        }

        [Fact(DisplayName = "QueryForListOfT_works")]
        public void QueryForListOfT_works()
        {
            var expected = new[] { new { Item1 = "azerty", Item2 = "qwerty" } }.ToList();
            string sql = "SELECT 'azerty','qwerty';";

            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                Assert.Equal(expected, WrappedConnectionEx.QueryForList(connection, sql, (r) => new { Item1 = r.GetString(0), Item2 = r.GetString(1) }));
                Assert.True(connection.DbConnection.State == ConnectionState.Closed);
            }
        }

        [Fact(DisplayName = "QueryForListOfT_never_returns_null")]
        public void QueryForListOfT_never_returns_null()
        {
            string sql = "SELECT tbl_name FROM sqlite_master WHERE type = 'PSG'";

            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                Assert.True(WrappedConnectionEx.QueryForList(connection, sql, (r) => new { Item1 = r.GetString(0) }).Count() == 0);
                Assert.True(connection.DbConnection.State == ConnectionState.Closed);
            }
        }
    }
}
