using System;
using System.Linq;
using Evolve.Dialect.SQLite;
using Evolve.Metadata;
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

        [Fact(DisplayName = "SaveMigration_works")]
        public void SaveMigration_works()
        {
            var migration = new MigrationScript(TestContext.ValidMigrationScriptPath, "1.0.0", "desc");

            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                connection.Open();
                var metadataTable = new SQLiteMetadataTable(TestContext.DefaultMetadataTableName, connection);
                metadataTable.SaveMigration(migration, true);
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
                metadataTable.SaveMigration(migrationScript, true);
                var migrationMetadata = metadataTable.GetAllMigrationMetadata().First();

                Assert.Equal(migrationScript.Description, migrationMetadata.Description);
                Assert.Equal(migrationScript.Name, migrationMetadata.Name);
                Assert.Equal(migrationScript.CalculateChecksum(), migrationMetadata.Checksum);
                Assert.Equal(migrationScript.Version, migrationMetadata.Version);
                Assert.Equal(true, migrationMetadata.Success);
                Assert.Equal(string.Empty, migrationMetadata.InstalledBy);
                Assert.True(migrationMetadata.Id > 0);
                Assert.True(migrationMetadata.InstalledOn.Date == DateTime.Now.Date);
            }
        }

        [Fact(DisplayName = "CanDropSchema_works")]
        public void CanDropSchema_works()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                connection.Open();
                var metadataTable = new SQLiteMetadataTable(TestContext.DefaultMetadataTableName, connection);
                Assert.False(metadataTable.CanDropSchema("main"));

                metadataTable.Save(MetadataType.NewSchema, "0", "New schema created.", "main");
                Assert.True(metadataTable.CanDropSchema("main"));
            }
        }

        [Fact(DisplayName = "CanCleanSchema_works")]
        public void CanCleanSchema_works()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                connection.Open();
                var metadataTable = new SQLiteMetadataTable(TestContext.DefaultMetadataTableName, connection);
                Assert.False(metadataTable.CanCleanSchema("main"));

                metadataTable.Save(MetadataType.EmptySchema, "0", "Schema is empty.", "main");
                Assert.True(metadataTable.CanCleanSchema("main"));
            }
        }

        [Fact(DisplayName = "FindStartVersion_works")]
        public void FindStartVersion_works()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                connection.Open();
                var metadataTable = new SQLiteMetadataTable(TestContext.DefaultMetadataTableName, connection);
                Assert.True(metadataTable.FindStartVersion() == new MigrationVersion("0"));

                metadataTable.Save(MetadataType.StartVersion, "1.0", "New starting version = 1.0");
                metadataTable.Save(MetadataType.StartVersion, "2.0", "New starting version = 2.0");
                Assert.True(metadataTable.FindStartVersion() == new MigrationVersion("2.0"));
            }
        }
    }
}
