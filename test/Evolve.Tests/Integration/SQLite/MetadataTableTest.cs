using System;
using System.Linq;
using Evolve.Dialect;
using Evolve.Metadata;
using Evolve.Migration;
using Xunit;

namespace Evolve.Tests.Integration.SQLite
{
    public class MetadataTableTest
    {
        const string MetadataTableName = "changelog";

        [Fact]
        public void When_not_exists_IsExists_returns_false()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", MetadataTableName);
                Assert.False(metadataTable.IsExists());
            }
        }

        [Fact]
        public void When_exists_IsExists_returns_true()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", MetadataTableName);

                metadataTable.CreateIfNotExists();
                Assert.True(metadataTable.IsExists());
            }
        }

        [Fact]
        public void When_not_exists_create_metadataTable()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", MetadataTableName);

                Assert.True(metadataTable.CreateIfNotExists());
                Assert.False(metadataTable.CreateIfNotExists());
            }
        }

        [Fact]
        public void SaveMigration_works()
        {
            var migration = new FileMigrationScript(TestContext.SQLite.ChinookScriptPath, "1.0.0", "desc");

            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", MetadataTableName);
                metadataTable.SaveMigration(migration, true);

                Assert.True(metadataTable.GetAllMigrationMetadata().First().Id > 0);
            }
        }

        [Fact]
        public void UpdateChecksum_works()
        {
            var migration = new FileMigrationScript(TestContext.SQLite.ChinookScriptPath, "1.0.0", "desc");

            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", MetadataTableName);
                metadataTable.SaveMigration(migration, true);

                var appliedMigration = metadataTable.GetAllMigrationMetadata().First();
                metadataTable.UpdateChecksum(appliedMigration.Id, "Hi !");
                Assert.Equal("Hi !", metadataTable.GetAllMigrationMetadata().First().Checksum);
            }
        }

        [Fact]
        public void GetAllMigrationMetadata_works()
        {
            var migrationScript = new FileMigrationScript(TestContext.SQLite.ChinookScriptPath, "1.0.0", "desc");

            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", MetadataTableName);
                metadataTable.SaveMigration(migrationScript, true);
                var migrationMetadata = metadataTable.GetAllMigrationMetadata().First();

                Assert.Equal(migrationScript.Description, migrationMetadata.Description);
                Assert.Equal(migrationScript.Name, migrationMetadata.Name);
                Assert.Equal(migrationScript.CalculateChecksum(), migrationMetadata.Checksum);
                Assert.Equal(migrationScript.Version, migrationMetadata.Version);
                Assert.True(migrationMetadata.Success);
                Assert.Equal(string.Empty, migrationMetadata.InstalledBy);
                Assert.True(migrationMetadata.Id > 0);
                Assert.True(migrationMetadata.InstalledOn.Date == DateTime.UtcNow.Date);
            }
        }

        [Fact]
        public void CanDropSchema_works()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", MetadataTableName);
                Assert.False(metadataTable.CanDropSchema("main"));

                metadataTable.Save(MetadataType.NewSchema, "0", "New schema created.", "main");
                Assert.True(metadataTable.CanDropSchema("main"));
            }
        }

        [Fact]
        public void CanEraseSchema_works()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", MetadataTableName);
                Assert.False(metadataTable.CanEraseSchema("main"));

                metadataTable.Save(MetadataType.EmptySchema, "0", "Schema is empty.", "main");
                Assert.True(metadataTable.CanEraseSchema("main"));
            }
        }

        [Fact]
        public void FindStartVersion_works()
        {
            using (var connection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
                var metadataTable = db.GetMetadataTable("", MetadataTableName);
                Assert.True(metadataTable.FindStartVersion() == MigrationVersion.MinVersion);

                metadataTable.Save(MetadataType.StartVersion, "1.0", "New starting version = 1.0", "");
                metadataTable.Save(MetadataType.StartVersion, "2.0", "New starting version = 2.0", "");
                Assert.True(metadataTable.FindStartVersion() == new MigrationVersion("2.0"));
            }
        }
    }
}
