using System;
using System.Linq;
using Evolve.Dialect;
using Evolve.Metadata;
using Evolve.Migration;
using Xunit;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.Sqlite
{
    public class MetadataTableTest
    {
        const string MetadataTableName = "changelog";

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void When_not_exists_IsExists_returns_false()
        {
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);
            Assert.False(metadataTable.IsExists());
        }

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void When_exists_IsExists_returns_true()
        {
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);

            metadataTable.CreateIfNotExists();
            Assert.True(metadataTable.IsExists());
        }

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void When_not_exists_create_metadataTable()
        {
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);

            Assert.True(metadataTable.CreateIfNotExists());
            Assert.False(metadataTable.CreateIfNotExists());
        }

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void Save_migration_works()
        {
            // Arrange
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);
            metadataTable.SaveMigration(FileMigrationScriptV, true);

            // Assert
            AssertMigrationMetadata(metadataTable.GetAllAppliedMigration().First());
        }

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void Save_repeatable_migration_works()
        {
            // Arrange
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);
            metadataTable.SaveMigration(FileMigrationScriptR, true);

            // Assert
            AssertMigrationMetadata(metadataTable.GetAllAppliedRepeatableMigration().First(),
                                    expectedName: "R__desc_b.sql",
                                    expectedDescription: "desc b",
                                    expectedVersion: null,
                                    expectedType: MetadataType.RepeatableMigration,
                                    expectedChecksum: "71568061B2970A4B7C5160FE75356E10");
        }

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void UpdateChecksum_works()
        {
            // Arrange
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);
            metadataTable.SaveMigration(FileMigrationScriptV, true);

            // Act
            var appliedMigration = metadataTable.GetAllAppliedMigration().First();
            metadataTable.UpdateChecksum(appliedMigration.Id, "Hi !");

            // Assert
            AssertMigrationMetadata(metadataTable.GetAllAppliedMigration().First(), expectedChecksum: "Hi !");
        }

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void GetAllMigrationMetadata_works()
        {
            // Arrange
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);
            metadataTable.SaveMigration(FileMigrationScriptV, true);
            metadataTable.SaveMigration(FileMigrationScriptR, true);

            // Assert
            Assert.Single(metadataTable.GetAllAppliedMigration());
            AssertMigrationMetadata(metadataTable.GetAllAppliedMigration().First());
        }

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void GetAllRepeatableMigrationMetadata_works()
        {
            // Arrange
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);
            metadataTable.SaveMigration(FileMigrationScriptV, true);
            metadataTable.SaveMigration(FileMigrationScriptR, true);

            // Assert
            Assert.Single(metadataTable.GetAllAppliedRepeatableMigration());
            AssertMigrationMetadata(metadataTable.GetAllAppliedRepeatableMigration().First(),
                    expectedId: 2,
                    expectedName: "R__desc_b.sql",
                    expectedDescription: "desc b",
                    expectedVersion: null,
                    expectedType: MetadataType.RepeatableMigration,
                    expectedChecksum: "71568061B2970A4B7C5160FE75356E10");
        }

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void CanDropSchema_works()
        {
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);
            Assert.False(metadataTable.CanDropSchema("main"));

            metadataTable.Save(MetadataType.NewSchema, "0", "New schema created.", "main");
            Assert.True(metadataTable.CanDropSchema("main"));
        }

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void CanEraseSchema_works()
        {
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);
            Assert.False(metadataTable.CanEraseSchema("main"));

            metadataTable.Save(MetadataType.EmptySchema, "0", "Schema is empty.", "main");
            Assert.True(metadataTable.CanEraseSchema("main"));
        }

        [Fact]
        [Category(Test.SQLite, Test.Metadata)]
        public void FindStartVersion_works()
        {
            using var connection = TestUtil.CreateSQLiteWrappedCnx();
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLite, connection);
            var metadataTable = db.GetMetadataTable("", MetadataTableName);
            Assert.True(metadataTable.FindStartVersion() == MigrationVersion.MinVersion);

            metadataTable.Save(MetadataType.StartVersion, "1.0", "New starting version = 1.0", "");
            metadataTable.Save(MetadataType.StartVersion, "2.0", "New starting version = 2.0", "");
            Assert.True(metadataTable.FindStartVersion() == new MigrationVersion("2.0"));
        }

        private void AssertMigrationMetadata(MigrationMetadata metadata,
                                             int expectedId = 1,
                                             string expectedName = "V2_3_1__Duplicate_migration_script.sql",
                                             string expectedVersion = "2.3.1",
                                             string expectedDescription = "Duplicate migration script",
                                             MetadataType expectedType = MetadataType.Migration,
                                             string expectedChecksum = "6C7E36422F79696602E19079534B4076")
        {
            Assert.Equal(expectedId, metadata.Id);
            Assert.Equal(expectedName, metadata.Name);
            Assert.Equal(expectedVersion, metadata.Version?.Label);
            Assert.Equal(expectedDescription, metadata.Description);
            Assert.Equal(expectedType, metadata.Type);
            Assert.Equal(expectedChecksum, metadata.Checksum);
            Assert.True(metadata.Success);
            Assert.Equal(string.Empty, metadata.InstalledBy);
            Assert.Equal(DateTime.UtcNow.Date, metadata.InstalledOn.Date);
        }
    }
}
