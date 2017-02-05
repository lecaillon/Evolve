using System.Linq;
using Evolve.Dialect.SQLite;
using Evolve.Migration;
using Xunit;

namespace Evolve.Test.Dialect.SQLite
{
    public class SQLiteMetadataTableTest
    {
        [Fact(DisplayName = "When_not_exists_create_metadataTable")]
        public void When_not_exists_create_metadataTable()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                connection.Open();
                var metadataTable = new SQLiteMetadataTable(TestContext.DefaultMetadataTableName, connection);

                Assert.True(metadataTable.CreateIfNotExists());
                Assert.False(metadataTable.CreateIfNotExists());
            }
        }

        [Fact(DisplayName = "AddMigrationMetadata_works")]
        public void AddMigrationMetadata_works()
        {
            var migration = new MigrationScript(TestContext.ValidMigrationScriptPath, "1.0.0", "desc");

            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                connection.Open();
                var metadataTable = new SQLiteMetadataTable(TestContext.DefaultMetadataTableName, connection);
                metadataTable.AddMigrationMetadata(migration, true);
            }
        }

        [Fact(DisplayName = "GetAllMigrationMetadata_works")]
        public void GetAllMigrationMetadata_works()
        {
            var migrationScript = new MigrationScript(TestContext.ValidMigrationScriptPath, "1.0.0", "desc");

            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                connection.Open();
                var metadataTable = new SQLiteMetadataTable(TestContext.DefaultMetadataTableName, connection);
                metadataTable.AddMigrationMetadata(migrationScript, true);
                var migrationMetadata = metadataTable.GetAllMigrationMetadata().First();

                Assert.Equal(migrationScript.Description, migrationMetadata.Description);
                Assert.Equal(migrationScript.Name, migrationMetadata.Name);
                Assert.Equal(migrationScript.Version, migrationMetadata.Version);
                Assert.Equal(true, migrationMetadata.Success);
                Assert.Equal(string.Empty, migrationMetadata.InstalledBy);
            }
        }
    }
}
