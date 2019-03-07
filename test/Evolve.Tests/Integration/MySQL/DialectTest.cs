using System;
using System.Data;
using System.Linq;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.MySQL;
using Evolve.Metadata;
using Evolve.Migration;
using Evolve.Tests.Infrastructure;
using Xunit;

namespace Evolve.Tests.Integration.MySQL
{
    [Collection("MySQL collection")]
    public class DialectTest
    {
        private readonly MySQLFixture _mySQLContainer;

        public DialectTest(MySQLFixture mySQLContainer)
        {
            _mySQLContainer = mySQLContainer;

            if (TestContext.Local)
            {
                mySQLContainer.Run(fromScratch: true);
            }
        }

        [Fact]
        [Category(Test.MySQL)]
        public void Run_all_MySQL_integration_tests_work()
        {
            // Open a connection to the MySQL database
            var cnn = _mySQLContainer.CreateDbConnection();
            cnn.Open();
            Assert.True(cnn.State == ConnectionState.Open, "Cannot open a connection to the database.");

            // Initiate a connection to the database
            var wcnn = new WrappedConnection(cnn);

            // Validate DBMS.MySQL_MariaDB
            Assert.Equal(DBMS.MySQL, wcnn.GetDatabaseServerType());

            // Init the DatabaseHelper
            DatabaseHelper db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.MySQL, wcnn);

            // Get default schema name
            Assert.True(db.GetCurrentSchemaName() == MySQLContainer.DbName, $"The default MySQL schema should be '{MySQLContainer.DbName}'.");

            // Create schema
            string schemaName = "My metadata schema";
            Schema schema = new MySQLSchema(schemaName, wcnn);
            Assert.False(schema.IsExists(), $"The schema [{schemaName}] should not already exist.");
            Assert.True(schema.Create(), $"Creation of the schema [{schemaName}] failed.");
            Assert.True(schema.IsExists(), $"The schema [{schemaName}] should be created.");
            Assert.True(schema.IsEmpty(), $"The schema [{schemaName}] should be empty.");

            // Get MetadataTable
            string metadataTableName = "change log";
            var metadataTable = db.GetMetadataTable(schemaName, metadataTableName);

            // Create MetadataTable
            Assert.False(metadataTable.IsExists(), "MetadataTable sould not already exist.");
            Assert.True(metadataTable.CreateIfNotExists(), "MetadataTable creation failed.");
            Assert.True(metadataTable.IsExists(), "MetadataTable sould exist.");
            Assert.False(metadataTable.CreateIfNotExists(), "MetadataTable already exists. Creation should return false.");
            Assert.True(metadataTable.GetAllMigrationMetadata().Count() == 0, "No migration metadata should be found.");

            // TryLock/ReleaseLock MetadataTable
            Assert.True(metadataTable.TryLock());
            Assert.True(metadataTable.ReleaseLock());

            // Save NewSchema metadata
            metadataTable.Save(MetadataType.NewSchema, "0", "New schema created.", schemaName);
            Assert.True(metadataTable.CanDropSchema(schemaName), $"[{schemaName}] should be droppable.");
            Assert.False(metadataTable.CanEraseSchema(schemaName), $"[{schemaName}] should not be erasable.");

            // Add metadata migration
            var migration = new FileMigrationScript(TestContext.MySQL.EmptyMigrationScriptPath, "1_3_2", "desc");
            metadataTable.SaveMigration(migration, true);
            var migrationMetadata = metadataTable.GetAllMigrationMetadata().FirstOrDefault();
            Assert.True(migrationMetadata != null, "One migration metadata should be found.");
            Assert.True(migrationMetadata.Version == migration.Version, "Metadata version is not the same.");
            Assert.True(migrationMetadata.Checksum == migration.CalculateChecksum(), "Metadata checksum is not the same.");
            Assert.True(migrationMetadata.Description == migration.Description, "Metadata descritpion is not the same.");
            Assert.True(migrationMetadata.Name == migration.Name, "Metadata name is not the same.");
            Assert.True(migrationMetadata.Success == true, "Metadata success is not true.");
            Assert.True(migrationMetadata.Id > 0, "Metadata id is not set.");
            Assert.True(migrationMetadata.InstalledOn.Date == DateTime.UtcNow.Date, $"Installed date is {migrationMetadata.InstalledOn.Date} whereas UtcNow is {DateTime.UtcNow.Date}.");

            // Update checksum
            metadataTable.UpdateChecksum(migrationMetadata.Id, "Hi !");
            Assert.Equal("Hi !", metadataTable.GetAllMigrationMetadata().First().Checksum);

            // Assert metadata schema is not empty
            Assert.False(schema.IsEmpty(), $"[{schemaName}] should not be empty.");

            // Erase schema
            schema.Erase();
            Assert.True(schema.IsEmpty(), $"The schema [{schemaName}] should be empty.");
            Assert.True(schema.IsExists(), $"The schema [{schemaName}] should exist.");

            // Drop schema
            schema.Drop();
            Assert.False(schema.IsExists(), $"The schema [{schemaName}] should not exist.");
        }
    }
}
