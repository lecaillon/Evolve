using Evolve.Dialect;
using Xunit;

namespace Evolve.Core.Test.Dialect.SQLite
{
    public class SQLiteDatabaseTest
    {
        [Fact(DisplayName = "SQLiteDatabase_name_is_sqlite")]
        public void SQLiteDatabase_name_is_sqlite()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                Assert.Equal("SQLite", db.DatabaseName);
            }
        }

        [Fact(DisplayName = "GetCurrentSchemaName_is_always_main")]
        public void GetCurrentSchemaName_is_always_main()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                Assert.Equal("main", db.GetCurrentSchemaName());
            }
        }

        [Fact(DisplayName = "ChangeSchema_always_returns_main")]
        public void ChangeSchema_always_returns_main()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var schema = db.ChangeSchema("another_shema");

                Assert.NotNull(schema);
                Assert.Equal("main", schema.Name);
            }
        }

        [Fact(DisplayName = "GetMetadataTable_works")]
        public void GetMetadataTable_works()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("main", TestContext.DefaultMetadataTableName);

                Assert.NotNull(metadataTable);
                Assert.True(metadataTable.CreateIfNotExists());
            }
        }
    }
}
