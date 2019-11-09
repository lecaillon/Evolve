using Evolve.Dialect;
using Xunit;

namespace Evolve.Tests.Integration.Sqlite
{
    public class DatabaseTest
    {
        [Fact]
        [Category(Test.SQLite)]
        public void SQLiteDatabase_name_is_sqlite()
        {
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            Assert.Equal("SQLite", db.DatabaseName);
        }

        [Fact]
        [Category(Test.SQLite)]
        public void GetCurrentSchemaName_is_always_main()
        {
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            Assert.Equal("main", db.GetCurrentSchemaName());
        }

        [Fact]
        [Category(Test.SQLite)]
        public void ChangeSchema_always_returns_main()
        {
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var schema = db.ChangeSchema("another_shema");

            Assert.NotNull(schema);
            Assert.Equal("main", schema.Name);
        }

        [Fact]
        [Category(Test.SQLite)]
        public void GetMetadataTable_works()
        {
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("main", "changelog");

            Assert.NotNull(metadataTable);
            Assert.True(metadataTable.CreateIfNotExists());
        }
    }
}
