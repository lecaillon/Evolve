using System.Collections.Generic;
using System.Linq;
using Evolve.Dialect;
using Xunit;

namespace Evolve.Tests.Extensions
{
    public class WrappedConnectionExtensionsTest
    {
        [Fact]
        public void GetDatabaseServerType_is_sqlite()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                Assert.Equal(DBMS.SQLite, WrappedConnectionEx.GetDatabaseServerType(connection));
            }
        }

        [Fact]
        public void QueryForLong_works()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                Assert.Equal(1L, WrappedConnectionEx.QueryForLong(connection, "SELECT 1;"));
            }
        }

        [Fact]
        public void QueryForString_works()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                Assert.Equal("azerty", WrappedConnectionEx.QueryForString(connection, "SELECT 'azerty';"));
            }
        }

        [Fact]
        public void QueryForListOfString_works()
        {
            var expected = new List<string> { "azerty", "qwerty" };
            string sql = "SELECT 'azerty' UNION SELECT 'qwerty';";

            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                Assert.Equal(expected, WrappedConnectionEx.QueryForListOfString(connection, sql));
            }
        }

        [Fact]
        public void QueryForListOfT_works()
        {
            var expected = new[] { new { Item1 = "azerty", Item2 = "qwerty" } }.ToList();
            string sql = "SELECT 'azerty','qwerty';";

            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                Assert.Equal(expected, WrappedConnectionEx.QueryForList(connection, sql, (r) => new { Item1 = r.GetString(0), Item2 = r.GetString(1) }));
            }
        }

        [Fact]
        public void QueryForListOfT_never_returns_null()
        {
            string sql = "SELECT tbl_name FROM sqlite_master WHERE type = 'PSG'";

            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                Assert.True(WrappedConnectionEx.QueryForList(connection, sql, (r) => new { Item1 = r.GetString(0) }).Count() == 0);
            }
        }
    }
}