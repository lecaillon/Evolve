using Evolve.Connection;
using Evolve.Dialect.SQLite;
using Xunit;

namespace Evolve.Tests.Integration.SQLite
{
    public class SchemaTest
    {
        [Fact]
        public void Can_get_schema()
        {
            using (var cnn = TestUtil.CreateSQLiteWrappedCnx())
            {
                var schema = new SQLiteSchema(cnn);

                Assert.True(schema.IsExists());
            }
        }

        [Fact]
        public void When_new_database_is_created_schema_is_empty()
        {
            using (var cnn = TestUtil.CreateSQLiteWrappedCnx())
            {
                var schema = new SQLiteSchema(cnn);

                Assert.True(schema.IsEmpty());
            }
        }

        [Fact]
        public void SQLite_does_not_support_creating_schemas()
        {
            using (var cnn = TestUtil.CreateSQLiteWrappedCnx())
            {
                var schema = new SQLiteSchema(cnn);

                Assert.False(schema.Create());
            }
        }

        [Fact]
        public void SQLite_does_not_support_dropping_schemas()
        {
            using (var cnn = TestUtil.CreateSQLiteWrappedCnx())
            {
                var schema = new SQLiteSchema(cnn);

                Assert.False(schema.Drop());
            }
        }

        [Fact]
        public void When_schema_contains_tables_schema_is_not_empty()
        {
            using (var cnn = TestUtil.CreateSQLiteWrappedCnx())
            {
                var schema = LoadChinookDatabase(cnn);

                Assert.False(schema.IsEmpty());
            }
        }

        [Fact]
        public void After_schema_cleannig_schema_is_empty()
        {
            using (var cnn = TestUtil.CreateSQLiteWrappedCnx())
            {
                var schema = LoadChinookDatabase(cnn);

                Assert.True(schema.Erase());
                Assert.True(schema.IsEmpty());
            }
        }

        [Fact]
        public void After_schema_cleannig_and_rollback_schema_is_not_empty()
        {
            using (var cnn = TestUtil.CreateSQLiteWrappedCnx())
            {
                var schema = LoadChinookDatabase(cnn);

                cnn.BeginTransaction();

                Assert.True(schema.Erase());
                Assert.True(schema.IsEmpty());

                cnn.Rollback();
                Assert.False(schema.IsEmpty());
            }
        }

        static SQLiteSchema LoadChinookDatabase(WrappedConnection cnn)
        {
            cnn.Open();
            using (var command = cnn.DbConnection.CreateCommand())
            {
                command.CommandText = TestContext.SQLite.ChinookScript;
                command.ExecuteNonQuery();
            }

            return new SQLiteSchema(cnn);
        }
    }
}
