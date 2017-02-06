using Evolve.Dialect.SQLite;
using Xunit;

namespace Evolve.Test.Dialect.SQLite
{
    public class SQLiteDatabaseTest
    {
        [Fact(DisplayName = "SQLiteDatabase_name_is_sqlite")]
        public void SQLiteDatabase_name_is_sqlite()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = new SQLiteDatabase(connection);
                Assert.Equal("sqlite", db.DatabaseName);
            }
        }
    }
}
