using Xunit;

namespace Evolve.Core.Test.Dialect.SQLite
{
    public class SQLiteSchemaTest
    {
        [Fact(DisplayName = "Can_get_schema")]
        public void Can_get_schema()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var schema = TestUtil.GetDefaultSQLiteSchema(connection);

                Assert.True(schema.IsExists());
            }
        }

        [Fact(DisplayName = "When_new_database_is_created_schema_is_empty")]
        public void When_new_database_is_created_schema_is_empty()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var schema = TestUtil.GetDefaultSQLiteSchema(connection);

                Assert.True(schema.IsEmpty());
            }
        }

        [Fact(DisplayName = "SQLite_does_not_support_creating_schemas")]
        public void SQLite_does_not_support_creating_schemas()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var schema = TestUtil.GetDefaultSQLiteSchema(connection);

                Assert.False(schema.Create());
            }
        }

        [Fact(DisplayName = "SQLite_does_not_support_dropping_schemas")]
        public void SQLite_does_not_support_dropping_schemas()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var schema = TestUtil.GetDefaultSQLiteSchema(connection);

                Assert.False(schema.Drop());
            }
        }

        [Fact(DisplayName = "When_schema_contains_tables_schema_is_not_empty")]
        public void When_schema_contains_tables_schema_is_not_empty()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var schema = TestUtil.LoadChinookDatabase(connection);

                Assert.False(schema.IsEmpty());
            }
        }

        [Fact(DisplayName = "After_schema_cleannig_schema_is_empty")]
        public void After_schema_cleannig_schema_is_empty()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var schema = TestUtil.LoadChinookDatabase(connection);

                Assert.True(schema.Erase());
                Assert.True(schema.IsEmpty());
            }
        }

        [Fact(DisplayName = "After_schema_cleannig_and_rollback_schema_is_not_empty")]
        public void After_schema_cleannig_and_rollback_schema_is_not_empty()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var schema = TestUtil.LoadChinookDatabase(connection);

                connection.BeginTransaction();

                Assert.True(schema.Erase());
                Assert.True(schema.IsEmpty());

                connection.Rollback();
                Assert.False(schema.IsEmpty());
            }
        }
    }
}
