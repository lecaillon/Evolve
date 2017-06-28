using System;
using System.Linq;
using Evolve.Dialect;
using Evolve.Metadata;
using Evolve.Migration;
using Xunit;

namespace Evolve.Core.Test.Dialect.SQLite
{
    public class SQLiteMetadataTableTest
    {
        [Fact(DisplayName = "When_not_exists_IsExists_returns_false")]
        public void When_not_exists_IsExists_returns_false()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", TestContext.DefaultMetadataTableName);
                Assert.False(metadataTable.IsExists());
            }
        }

        [Fact(DisplayName = "When_exists_IsExists_returns_true")]
        public void When_exists_IsExists_returns_true()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", TestContext.DefaultMetadataTableName);

                metadataTable.CreateIfNotExists();
                Assert.True(metadataTable.IsExists());
            }
        }

        [Fact(DisplayName = "When_not_exists_create_metadataTable")]
        public void When_not_exists_create_metadataTable()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", TestContext.DefaultMetadataTableName);

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
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", TestContext.DefaultMetadataTableName);
                metadataTable.SaveMigration(migration, true);

                Assert.NotNull(metadataTable.GetAllMigrationMetadata().First().Id > 0);
            }
        }

        [Fact(DisplayName = "UpdateChecksum_works")]
        public void UpdateChecksum_works()
        {
            var migration = new MigrationScript(TestContext.ValidMigrationScriptPath, "1.0.0", "desc");

            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", TestContext.DefaultMetadataTableName);
                metadataTable.SaveMigration(migration, true);

                var appliedMigration = metadataTable.GetAllMigrationMetadata().First();
                metadataTable.UpdateChecksum(appliedMigration.Id, "Hi !");
                Assert.Equal("Hi !", metadataTable.GetAllMigrationMetadata().First().Checksum);
            }
        }

        [Fact(DisplayName = "GetAllMigrationMetadata_works")]
        public void GetAllMigrationMetadata_works()
        {
            var migrationScript = new MigrationScript(TestContext.ValidMigrationScriptPath, "1.0.0", "desc");

            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", TestContext.DefaultMetadataTableName);
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
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", TestContext.DefaultMetadataTableName);
                Assert.False(metadataTable.CanDropSchema("main"));

                metadataTable.Save(MetadataType.NewSchema, "0", "New schema created.", "main");
                Assert.True(metadataTable.CanDropSchema("main"));
            }
        }

        [Fact(DisplayName = "CanEraseSchema_works")]
        public void CanEraseSchema_works()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", TestContext.DefaultMetadataTableName);
                Assert.False(metadataTable.CanEraseSchema("main"));

                metadataTable.Save(MetadataType.EmptySchema, "0", "Schema is empty.", "main");
                Assert.True(metadataTable.CanEraseSchema("main"));
            }
        }

        [Fact(DisplayName = "FindStartVersion_works")]
        public void FindStartVersion_works()
        {
            using (var connection = TestUtil.GetInMemorySQLiteWrappedConnection())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", TestContext.DefaultMetadataTableName);
                Assert.True(metadataTable.FindStartVersion() == MigrationVersion.MinVersion);

                metadataTable.Save(MetadataType.StartVersion, "1.0", "New starting version = 1.0", "");
                metadataTable.Save(MetadataType.StartVersion, "2.0", "New starting version = 2.0", "");
                Assert.True(metadataTable.FindStartVersion() == new MigrationVersion("2.0"));
            }
        }
    }
}
