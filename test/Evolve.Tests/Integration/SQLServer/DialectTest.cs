using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.SQLServer;
using Evolve.Metadata;
using Evolve.Migration;
using Evolve.Tests.Infrastructure;
using Xunit;

namespace Evolve.Tests.Integration.SQLServer
{
    [Collection("SQLServer collection")]
    public class DialectTest
    {
        public const string DbName = "my_database_1";
        private readonly SQLServerFixture _sqlServerContainer;

        public DialectTest(SQLServerFixture sqlServerContainer)
        {
            _sqlServerContainer = sqlServerContainer;

            if (TestContext.Local || TestContext.AzureDevOps)
            {
                sqlServerContainer.Run(fromScratch: false);
            }

            TestUtil.CreateSqlServerDatabase(DbName, _sqlServerContainer.GetCnxStr("master"));
        }

        [Fact]
        public void Run_all_SQLServer_integration_tests_work()
        {
            // Open a connection to the SQLServer database
            var cnn = new SqlConnection(_sqlServerContainer.GetCnxStr(DbName));
            cnn.Open();
            Assert.True(cnn.State == ConnectionState.Open, "Cannot open a connection to the database.");

            // Initiate a connection to the database
            var wcnn = new WrappedConnection(cnn);

            // Validate DBMS.SQLServer
            Assert.Equal(DBMS.SQLServer, wcnn.GetDatabaseServerType());

            // Init the DatabaseHelper
            DatabaseHelper db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLServer, wcnn);

            // Get default schema name
            string metadataSchemaName = db.GetCurrentSchemaName();
            Assert.True(metadataSchemaName == "dbo", "The default SQLServer schema should be 'dbo'.");

            // Get MetadataTable
            string metadataTableName = "changelog";
            var metadata = db.GetMetadataTable(metadataSchemaName, metadataTableName);

            // Create MetadataTable
            Assert.False(metadata.IsExists(), "MetadataTable sould not already exist.");
            Assert.True(metadata.CreateIfNotExists(), "MetadataTable creation failed.");
            Assert.True(metadata.IsExists(), "MetadataTable sould exist.");
            Assert.False(metadata.CreateIfNotExists(), "MetadataTable already exists. Creation should return false.");
            Assert.True(metadata.GetAllMigrationMetadata().Count() == 0, "No migration metadata should be found.");

            // TryLock/ReleaseLock MetadataTable
            Assert.True(metadata.TryLock());
            Assert.True(metadata.ReleaseLock());

            // Save EmptySchema metadata
            metadata.Save(MetadataType.EmptySchema, "0", "Empty schema found.", metadataSchemaName);
            Assert.False(metadata.CanDropSchema(metadataSchemaName), $"[{metadataSchemaName}] should not be droppable.");
            Assert.True(metadata.CanEraseSchema(metadataSchemaName), $"[{metadataSchemaName}] should be erasable.");

            // Add metadata migration
            var migrationScript = new FileMigrationScript(TestContext.SqlServer.EmptyMigrationScriptPath, "1_3_2", "Migration_description");
            metadata.SaveMigration(migrationScript, true);
            var migrationMetadata = metadata.GetAllMigrationMetadata().FirstOrDefault();
            Assert.True(migrationMetadata != null, "One migration metadata should be found.");
            Assert.True(migrationMetadata.Version == migrationScript.Version, "Metadata version is not the same.");
            Assert.True(migrationMetadata.Checksum == migrationScript.CalculateChecksum(), "Metadata checksum is not the same.");
            Assert.True(migrationMetadata.Description == migrationScript.Description, "Metadata descritpion is not the same.");
            Assert.True(migrationMetadata.Name == migrationScript.Name, "Metadata name is not the same.");
            Assert.True(migrationMetadata.Success == true, "Metadata success is not true.");
            Assert.True(migrationMetadata.Id > 0, "Metadata id is not set.");
            Assert.True(migrationMetadata.InstalledOn.Date == DateTime.UtcNow.Date, "Installed date is not set.");

            // Update checksum
            metadata.UpdateChecksum(migrationMetadata.Id, "Hi !");
            Assert.Equal("Hi !", metadata.GetAllMigrationMetadata().First().Checksum);

            // Assert metadata schema is not empty
            Schema metadataSchema = new SQLServerSchema(metadataSchemaName, wcnn);
            Assert.False(metadataSchema.IsEmpty(), $"[{metadataSchemaName}] should not be empty.");

            // Erase schema
            metadataSchema.Erase();
            Assert.True(metadataSchema.IsEmpty(), $"The schema [{metadataSchemaName}] should be empty.");
            Assert.True(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should exist.");

            // Acquisition du lock applicatif
            while (true)
            {
                if (db.TryAcquireApplicationLock())
                {
                    break;
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Assert.True(db.TryAcquireApplicationLock(), "Cannot acquire application lock.");

            // Can not acquire lock while it is taken by another connection
            var cnn2 = new SqlConnection(_sqlServerContainer.GetCnxStr(DbName));
            var wcnn2 = new WrappedConnection(cnn2);
            var db2 = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLServer, wcnn2);
            Assert.False(db2.TryAcquireApplicationLock(), "Application lock could not have been acquired.");

            // Release the lock
            db.ReleaseApplicationLock();
            db.CloseConnection();
            Assert.True(db.WrappedConnection.DbConnection.State == ConnectionState.Closed, "SQL connection should be closed.");
        }
    }
}
