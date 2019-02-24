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

            if (TestContext.Local)
            {
                sqlServerContainer.Run(fromScratch: true);
            }

            TestUtil.CreateSqlServerDatabase(DbName, _sqlServerContainer.GetCnxStr("master"));
        }

        [Fact]
        [Category(Test.SQLServer)]
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
            string schemaName = db.GetCurrentSchemaName();
            Assert.True(schemaName == "dbo", "The default SQLServer schema should be 'dbo'.");

            // Get MetadataTable
            string metadataTableName = "changelog";
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

            // Save EmptySchema metadata
            metadataTable.Save(MetadataType.EmptySchema, "0", "Empty schema found.", schemaName);
            Assert.False(metadataTable.CanDropSchema(schemaName), $"[{schemaName}] should not be droppable.");
            Assert.True(metadataTable.CanEraseSchema(schemaName), $"[{schemaName}] should be erasable.");

            // Add metadata migration
            var migration = new FileMigrationScript(TestContext.SqlServer.EmptyMigrationScriptPath, "1_3_2", "desc", MetadataType.Migration);
            metadataTable.SaveMigration(migration, true);
            var migrationMetadata = metadataTable.GetAllMigrationMetadata().FirstOrDefault();
            Assert.True(migrationMetadata != null, "One migration metadata should be found.");
            Assert.True(migrationMetadata.Version == migration.Version, "Metadata version is not the same.");
            Assert.True(migrationMetadata.Checksum == migration.CalculateChecksum(), "Metadata checksum is not the same.");
            Assert.True(migrationMetadata.Description == migration.Description, "Metadata descritpion is not the same.");
            Assert.True(migrationMetadata.Name == migration.Name, "Metadata name is not the same.");
            Assert.True(migrationMetadata.Success == true, "Metadata success is not true.");
            Assert.True(migrationMetadata.Id > 0, "Metadata id is not set.");
            Assert.True(migrationMetadata.InstalledOn.Date == DateTime.UtcNow.Date, $"Metadata InstalledOn date {migrationMetadata.InstalledOn} must be equals to {DateTime.UtcNow.Date}.");

            // Update checksum
            metadataTable.UpdateChecksum(migrationMetadata.Id, "Hi !");
            Assert.Equal("Hi !", metadataTable.GetAllMigrationMetadata().First().Checksum);

            // Assert metadata schema is not empty
            Schema schema = new SQLServerSchema(schemaName, wcnn);
            Assert.False(schema.IsEmpty(), $"[{schemaName}] should not be empty.");

            // Erase schema
            schema.Erase();
            Assert.True(schema.IsEmpty(), $"The schema [{schemaName}] should be empty.");
            Assert.True(schema.IsExists(), $"The schema [{schemaName}] should exist.");

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
