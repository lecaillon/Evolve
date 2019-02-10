﻿using Evolve.Dialect;
using Xunit;

namespace Evolve.Tests.Integration.SQLite
{
    public class DatabaseTest
    {
        [Fact]
        public void SQLiteDatabase_name_is_sqlite()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                Assert.Equal("SQLite", db.DatabaseName);
            }
        }

        [Fact]
        public void GetCurrentSchemaName_is_always_main()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                Assert.Equal("main", db.GetCurrentSchemaName());
            }
        }

        [Fact]
        public void ChangeSchema_always_returns_main()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var schema = db.ChangeSchema("another_shema");

                Assert.NotNull(schema);
                Assert.Equal("main", schema.Name);
            }
        }

        [Fact]
        public void GetMetadataTable_works()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("main", "changelog");

                Assert.NotNull(metadataTable);
                Assert.True(metadataTable.CreateIfNotExists());
            }
        }
    }
}
