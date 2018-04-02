using Cassandra.Data;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.Cassandra;
using Evolve.Metadata;
using Evolve.Migration;
using Evolve.Test.Utilities;
using System;
using System.Data;
using System.Linq;
using Xunit;

namespace Evolve.IntegrationTest.Cassandra
{
    [Collection("Database collection")]
    public class DialectTest
    {
        private readonly CassandraFixture _cassandraFixture;

        public DialectTest(CassandraFixture cassandraFixture)
        {
            _cassandraFixture = cassandraFixture;

            if (!TestContext.Travis && !TestContext.AppVeyor)
            { // AppVeyor and Windows 2016 does not support linux docker images
                cassandraFixture.Start(fromScratch: true);
            }
        }

        [SkipOnAppVeyorFact(DisplayName = "Run_all_Cassandra_integration_tests_work")]
        public void Run_all_Cassandra_integration_tests_work()
        {
            // Open a connection to Cassandra
            var cnn = new CqlConnection($"Contact Points=127.0.0.1;Port={_cassandraFixture.Cassandra.HostPort};Cluster Name={_cassandraFixture.Cassandra.ClusterName}");
            cnn.Open();
            Assert.True(cnn.State == ConnectionState.Open, "Cannot open a connection to Cassandra.");

            // Initiate a connection to the database
            var wcnn = new WrappedConnection(cnn);

            // Validate DBMS.Cassandra
            Assert.Equal(DBMS.Cassandra, wcnn.GetDatabaseServerType());

            // Init the DatabaseHelper
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.Cassandra, wcnn);

            // Create schema
            string metadataKeyspaceName = "my_metadata_keyspace";
            Schema metadataSchema = new CassandraKeyspace(metadataKeyspaceName, CassandraKeyspace.CreateSimpleStrategy(1), wcnn);
            Assert.False(metadataSchema.IsExists(), $"The schema [{metadataKeyspaceName}] should not already exist.");
            Assert.True(metadataSchema.Create(), $"Creation of the schema [{metadataKeyspaceName}] failed.");
            Assert.True(metadataSchema.IsExists(), $"The schema [{metadataKeyspaceName}] should be created.");
            Assert.True(metadataSchema.IsEmpty(), $"The schema [{metadataKeyspaceName}] should be empty.");

            var s = db.GetSchema("my_metadata_keyspace");

            // Get MetadataTable
            string metadataTableName = "change_log";
            var metadata = db.GetMetadataTable(metadataKeyspaceName, metadataTableName);

            // Create MetadataTable
            Assert.False(metadata.IsExists(), "MetadataTable sould not already exist.");
            Assert.True(metadata.CreateIfNotExists(), "MetadataTable creation failed.");
            Assert.True(metadata.IsExists(), "MetadataTable sould exist.");
            Assert.False(metadata.CreateIfNotExists(), "MetadataTable already exists. Creation should return false.");
            Assert.True(metadata.GetAllMigrationMetadata().Count() == 0, "No migration metadata should be found.");

            // TryLock/ReleaseLock MetadataTable
            Assert.True(metadata.TryLock());
            Assert.True(metadata.ReleaseLock());

            // Save NewSchema metadata
            metadata.Save(MetadataType.NewSchema, "0", "New schema created.", metadataKeyspaceName);
            Assert.True(metadata.CanDropSchema(metadataKeyspaceName), $"[{metadataKeyspaceName}] should be droppable.");
            Assert.False(metadata.CanEraseSchema(metadataKeyspaceName), $"[{metadataKeyspaceName}] should not be erasable.");

            // Add metadata migration
            var migrationScript = new FileMigrationScript(TestContext.EmptyMigrationScriptPath, "1_3_2", "Migration_description");
            metadata.SaveMigration(migrationScript, true);
            var migrationMetadata = metadata.GetAllMigrationMetadata().FirstOrDefault();
            Assert.True(migrationMetadata != null, "One migration metadata should be found.");
            Assert.True(migrationMetadata.Version == migrationScript.Version, "Metadata version is not the same.");
            Assert.True(migrationMetadata.Checksum == migrationScript.CalculateChecksum(), "Metadata checksum is not the same.");
            Assert.True(migrationMetadata.Description == migrationScript.Description, "Metadata descritpion is not the same.");
            Assert.True(migrationMetadata.Name == migrationScript.Name, "Metadata name is not the same.");
            Assert.True(migrationMetadata.Success == true, "Metadata success is not true.");
            Assert.True(migrationMetadata.Id != 0, "Metadata id is not set.");
            Assert.True(migrationMetadata.InstalledOn.Date == DateTime.UtcNow.Date, $"Installed date is {migrationMetadata.InstalledOn.Date} whereas UtcNow is {DateTime.UtcNow.Date}.");

            // Update checksum
            metadata.UpdateChecksum(migrationMetadata.Id, "Hi !");
            Assert.Equal("Hi !", metadata.GetAllMigrationMetadata().First().Checksum);

            // Assert metadata schema is not empty
            Assert.False(metadataSchema.IsEmpty(), $"[{metadataKeyspaceName}] should not be empty.");

            // Erase schema
            metadataSchema.Erase();
            Assert.True(metadataSchema.IsEmpty(), $"The schema [{metadataKeyspaceName}] should be empty.");
            Assert.True(metadataSchema.IsExists(), $"The schema [{metadataKeyspaceName}] should exist.");

            // Drop schema
            metadataSchema.Drop();
            Assert.False(metadataSchema.IsExists(), $"The schema [{metadataKeyspaceName}] should not exist.");
        }
    }
}
