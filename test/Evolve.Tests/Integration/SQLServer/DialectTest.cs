using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.SQLServer;
using Evolve.Metadata;
using Evolve.Tests.Infrastructure;
using Xunit;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.SQLServer
{
    [Collection("SQLServer collection")]
    public class DialectTest
    {
        public const string DbName = "my_database_1";
        private readonly SQLServerFixture _dbContainer;

        public DialectTest(SQLServerFixture dbContainer)
        {
            _dbContainer = dbContainer;

            if (Local)
            {
                dbContainer.Run(fromScratch: true);
            }

            TestUtil.CreateSqlServerDatabase(DbName, _dbContainer.GetCnxStr("master"));
        }

        [Fact]
        [Category(Test.SQLServer)]
        public void Run_all_SQLServer_integration_tests_work()
        {
            // Open a connection to SQL Server
            var cnn = new SqlConnection(_dbContainer.GetCnxStr(DbName));
            cnn.Open();
            Assert.True(cnn.State == ConnectionState.Open, "Cannot open a connection to the database.");

            // Initialize the Evolve connection wrapper
            var wcnn = new WrappedConnection(cnn);

            // Assert the DBMS type
            Assert.Equal(DBMS.SQLServer, wcnn.GetDatabaseServerType());

            // Initialize the DatabaseHelper
            DatabaseHelper db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLServer, wcnn);

            // Assert the default schema name
            string schemaName = db.GetCurrentSchemaName();
            Assert.True(schemaName == "dbo", "The default SQLServer schema should be 'dbo'.");

            // Assert MetadataTable creation
            var metadataTable = db.GetMetadataTable(schemaName, "changelog");
            Assert.False(metadataTable.IsExists(), "MetadataTable sould not already exist.");
            Assert.True(metadataTable.CreateIfNotExists(), "MetadataTable creation failed.");
            Assert.True(metadataTable.IsExists(), "MetadataTable should exist.");
            Assert.False(metadataTable.CreateIfNotExists(), "MetadataTable already exists. Creation should return false.");
            Assert.True(metadataTable.GetAllMigrationMetadata().Count() == 0, "No migration metadata should be found.");
            Assert.True(metadataTable.GetAllRepeatableMigrationMetadata().Count() == 0, "No repeatable migration metadata should be found.");

            // Assert TryLock/ReleaseLock
            Assert.True(metadataTable.TryLock());
            Assert.True(metadataTable.ReleaseLock());

            // Assert save EmptySchema metadata
            metadataTable.Save(MetadataType.EmptySchema, "0", "Empty schema found.", schemaName);
            Assert.False(metadataTable.CanDropSchema(schemaName), $"[{schemaName}] should not be droppable.");
            Assert.True(metadataTable.CanEraseSchema(schemaName), $"[{schemaName}] should be erasable.");

            // Assert save migration
            metadataTable.SaveMigration(FileMigrationScriptV, true);
            Assert.True(metadataTable.GetAllMigrationMetadata().Count() == 1, $"1 migration metadata should have been found, instead of {metadataTable.GetAllMigrationMetadata().Count()}.");
            Assert.True(metadataTable.GetAllRepeatableMigrationMetadata().Count() == 0, $"0 repeatable migration metadata should have been found, instead of {metadataTable.GetAllRepeatableMigrationMetadata().Count()}.");
            var metadata = metadataTable.GetAllMigrationMetadata().First();
            Assert.True(metadata.Version == FileMigrationScriptV.Version, $"Migration metadata version should be: 2.3.1, but found {metadata.Version}.");
            Assert.True(metadata.Checksum == FileMigrationScriptV.CalculateChecksum(), $"Migration metadata checksum should be; 6C7E36422F79696602E19079534B4076, but found {metadata.Checksum}.");
            Assert.True(metadata.Description == FileMigrationScriptV.Description, $"Migration metadata description should be: Duplicate migration script, but found {metadata.Description}.");
            Assert.True(metadata.Name == FileMigrationScriptV.Name, $"Migration metadata name should be: V2_3_1__Duplicate_migration_script.sql, but found {metadata.Name}.");
            Assert.True(metadata.Success == true, $"Migration metadata success should be: true, but found {metadata.Success}.");
            Assert.True(metadata.Id == 2, $"Migration metadata id should be: 2, but found {metadata.Id}.");
            Assert.True(metadata.Type == MetadataType.Migration, $"Migration metadata type should be: Migration, but found {metadata.Type}.");
            Assert.True(metadata.InstalledOn.Date == DateTime.UtcNow.Date, $"Migration metadata InstalledOn date {metadata.InstalledOn} should be equals to {DateTime.UtcNow.Date}.");

            // Assert updated checksum
            metadataTable.UpdateChecksum(metadata.Id, "Hi !");
            metadata = metadataTable.GetAllMigrationMetadata().First();
            Assert.True(metadata.Checksum == "Hi !", $"Updated checksum should be: Hi!, but found {metadata.Checksum}");

            // Assert save repeatable migration
            metadataTable.SaveMigration(FileMigrationScriptR, true);
            Assert.True(metadataTable.GetAllMigrationMetadata().Count() == 1, $"1 migration metadata should have been found, instead of {metadataTable.GetAllMigrationMetadata().Count()}.");
            Assert.True(metadataTable.GetAllRepeatableMigrationMetadata().Count() == 1, $"1 repeatable migration metadata should have been found, instead of {metadataTable.GetAllRepeatableMigrationMetadata().Count()}.");
            metadata = metadataTable.GetAllRepeatableMigrationMetadata().First();
            Assert.True(metadata.Version == FileMigrationScriptR.Version, $"Repeatable migration metadata version should be: null, but found {metadata.Version}.");
            Assert.True(metadata.Checksum == FileMigrationScriptR.CalculateChecksum(), $"Repeatable migration metadata checksum should be; 71568061B2970A4B7C5160FE75356E10, but found {metadata.Checksum}.");
            Assert.True(metadata.Description == FileMigrationScriptR.Description, $"Repeatable migration metadata description should be: desc b, but found {metadata.Description}.");
            Assert.True(metadata.Name == FileMigrationScriptR.Name, $"Repeatable migration metadata name should be: R__desc_b.sql, but found {metadata.Name}.");
            Assert.True(metadata.Success == true, $"Repeatable migration metadata success should be: true, but found {metadata.Success}.");
            Assert.True(metadata.Id == 3, $"Repeatable migration metadata id should be: 3, but found {metadata.Id}.");
            Assert.True(metadata.Type == MetadataType.RepeatableMigration, $"Repeatable migration metadata type should be: RepeatableMigration, but found {metadata.Type}.");
            Assert.True(metadata.InstalledOn.Date == DateTime.UtcNow.Date, $"Repeatable migration metadata InstalledOn date {metadata.InstalledOn} should be equals to {DateTime.UtcNow.Date}.");

            // Assert metadata schema is not empty
            Schema schema = new SQLServerSchema(schemaName, wcnn);
            Assert.False(schema.IsEmpty(), $"[{schemaName}] should not be empty.");

            // Assert erase of the schema
            schema.Erase();
            Assert.True(schema.IsEmpty(), $"The schema [{schemaName}] should be empty.");
            Assert.True(schema.IsExists(), $"The schema [{schemaName}] should exist.");

            // Assert lock acquisition
            Assert.True(db.TryAcquireApplicationLock(), "Cannot acquire application lock.");

            // Can not acquire lock while it is taken by another connection
            var cnn2 = new SqlConnection(_dbContainer.GetCnxStr(DbName));
            var wcnn2 = new WrappedConnection(cnn2);
            var db2 = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLServer, wcnn2);
            Assert.False(db2.TryAcquireApplicationLock(), "Application lock should not have been acquired, because it is already handled.");

            // Assert lock is released
            Assert.True(db.ReleaseApplicationLock(), "Cannot release the application lock.");

            // Assert connection is closed
            db.CloseConnection();
            Assert.True(db.WrappedConnection.DbConnection.State == ConnectionState.Closed, "Database connection should be closed.");
        }
    }
}
