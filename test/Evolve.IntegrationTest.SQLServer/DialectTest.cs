using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.SQLServer;
using Evolve.Metadata;
using Evolve.Migration;
using Xunit;

namespace Evolve.IntegrationTest.SQLServer
{
    public class DialectTest : IDisposable
    {
        public const string DbName = "my_database_1";

        [Fact(DisplayName = "Run_all_SQLServer_integration_tests_work")]
        public void Run_all_SQLServer_integration_tests_work()
        {
            // Open a connection to the SQLServer database
            var cnn = new SqlConnection($"Server=127.0.0.1;Database={DbName};User Id={TestContext.DbUser};Password={TestContext.DbPwd};");
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

            // Lock MetadataTable
            metadata.Lock();

            // Save EmptySchema metadata
            metadata.Save(MetadataType.EmptySchema, "0", "Empty schema found.", metadataSchemaName);
            Assert.False(metadata.CanDropSchema(metadataSchemaName), $"[{metadataSchemaName}] should not be droppable.");
            Assert.True(metadata.CanEraseSchema(metadataSchemaName), $"[{metadataSchemaName}] should be erasable.");

            // Add metadata migration
            var migrationScript = new MigrationScript(TestContext.EmptyMigrationScriptPath, "1_3_2", "Migration_description");
            metadata.SaveMigration(migrationScript, true);
            var migrationMetadata = metadata.GetAllMigrationMetadata().FirstOrDefault();
            Assert.True(migrationMetadata != null, "One migration metadata should be found.");
            Assert.True(migrationMetadata.Version == migrationScript.Version, "Metadata version is not the same.");
            Assert.True(migrationMetadata.Checksum == migrationScript.CalculateChecksum(), "Metadata checksum is not the same.");
            Assert.True(migrationMetadata.Description == migrationScript.Description, "Metadata descritpion is not the same.");
            Assert.True(migrationMetadata.Name == migrationScript.Name, "Metadata name is not the same.");
            Assert.True(migrationMetadata.Success == true, "Metadata success is not true.");
            Assert.True(migrationMetadata.Id > 0, "Metadata id is not set.");
            // Assert.True(migrationMetadata.InstalledOn.Date == DateTime.Now.Date, "Installed date is not set.");

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
        }

        /// <summary>
        ///     Start SQLServer server.
        /// </summary>
        public DialectTest()
        {
            TestUtil.RunContainer();
            TestUtil.CreateTestDatabase(DbName);
        }

        /// <summary>
        ///     Stop SQLServer server and remove container.
        /// </summary>
        public void Dispose()
        {
            TestUtil.RemoveContainer();
        }
    }
}
