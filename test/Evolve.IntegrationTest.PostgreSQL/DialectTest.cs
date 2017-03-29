using System;
using System.Data;
using System.Linq;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.PostgreSQL;
using Evolve.Metadata;
using Evolve.Migration;
using Npgsql;
using Xunit;

namespace Evolve.IntegrationTest.PostgreSQL
{
    public class DialectTest : IDisposable
    {
        [Fact(DisplayName = "Run_all_PostgreSQL_integration_tests_work")]
        public void Run_all_PostgreSQL_integration_tests_work()
        {
            // Open a connection to the PostgreSQL database
            var cnn = new NpgsqlConnection($"Server=127.0.0.1;Port={TestContext.ContainerPort};Database={TestContext.DbName};User Id={TestContext.DbUser};Password={TestContext.DbPwd};");
            cnn.Open();
            Assert.True(cnn.State == ConnectionState.Open, "Cannot open a connection to the database.");

            // Initiate a connection to the database
            var wcnn = new WrappedConnection(cnn);

            // Validate DBMS.PostgreSQL
            Assert.Equal(DBMS.PostgreSQL, wcnn.GetDatabaseServerType());

            // Init the DatabaseHelper
            DatabaseHelper db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.PostgreSQL, wcnn);

            // Test default schema name
            Assert.True(db.GetCurrentSchemaName() == "public", "The default PostgreSQL schema should be 'public'.");

            // Create schema
            string metadataSchemaName = "My metadata schema";
            Schema metadataSchema = new PostgreSQLSchema(metadataSchemaName, wcnn);
            Assert.False(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should not already exist.");
            Assert.True(metadataSchema.Create(), $"Creation of the schema [{metadataSchemaName}] failed.");
            Assert.True(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should be created.");
            Assert.True(metadataSchema.IsEmpty(), $"The schema [{metadataSchemaName}] should be empty.");

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

            // Save NewSchema metadata
            metadata.Save(MetadataType.NewSchema, "0", "New schema created.", metadataSchemaName);
            Assert.True(metadata.CanDropSchema(metadataSchemaName), $"[{metadataSchemaName}] should be droppable.");
            Assert.False(metadata.CanEraseSchema(metadataSchemaName), $"[{metadataSchemaName}] should not be erasable.");

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
            Assert.True(migrationMetadata.InstalledOn.Date == DateTime.Now.Date, "Installed date is not set.");

            // Update checksum
            metadata.UpdateChecksum(migrationMetadata.Id, "Hi !");
            Assert.Equal("Hi !", metadata.GetAllMigrationMetadata().First().Checksum);

            // Assert metadata schema is not empty
            Assert.False(metadataSchema.IsEmpty(), $"[{metadataSchemaName}] should not be empty.");

            // Erase schema
            metadataSchema.Erase();
            Assert.True(metadataSchema.IsEmpty(), $"The schema [{metadataSchemaName}] should be empty.");
            Assert.True(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should exist.");

            // Drop schema
            metadataSchema.Drop();
            Assert.False(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should not exist.");
        }

        /// <summary>
        ///     Start PostgreSQL server.
        /// </summary>
        public DialectTest()
        {
            TestUtil.RunContainer();
        }

        /// <summary>
        ///     Stop PostgreSQL server and remove container.
        /// </summary>
        public void Dispose()
        {
            TestUtil.RemoveContainer();
        }
    }
}
