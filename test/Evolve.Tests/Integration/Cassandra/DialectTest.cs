using System;
using System.Data;
using System.Linq;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.Cassandra;
using Evolve.Metadata;
using Evolve.Migration;
using Evolve.Tests.Infrastructure;
using Xunit;

namespace Evolve.Tests.Integration.Cassandra
{
    public class DialectTest
    {
        /// <summary>
        ///     Second part of the integration test.
        /// </summary>
        /// <remarks>
        ///     Due to some issues running 2 Cassandra containers, one after the other, 
        ///     in the same test context, we merge the integration tests to only use one container.
        ///     My guess, a possible Cassandra driver issue.
        /// </remarks>
        public static void Run_all_Cassandra_integration_tests_work(CassandraFixture cassandraContainer)
        {
            // Open a connection to Cassandra
            var cnn = cassandraContainer.CreateDbConnection();
            cnn.Open();
            Assert.True(cnn.State == ConnectionState.Open, "Cannot open a connection to Cassandra.");

            // Initiate a connection to the database
            var wcnn = new WrappedConnection(cnn);

            // Validate DBMS.Cassandra
            Assert.Equal(DBMS.Cassandra, wcnn.GetDatabaseServerType());

            // Init the DatabaseHelper
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.Cassandra, wcnn);

            // Create schema
            string keyspaceName = "my_keyspace_2";
            Schema schema = new CassandraKeyspace(keyspaceName, CassandraKeyspace.CreateSimpleStrategy(1), wcnn);
            Assert.False(schema.IsExists(), $"The schema [{keyspaceName}] should not already exist.");
            Assert.True(schema.Create(), $"Creation of the schema [{keyspaceName}] failed.");
            Assert.True(schema.IsExists(), $"The schema [{keyspaceName}] should be created.");
            Assert.True(schema.IsEmpty(), $"The schema [{keyspaceName}] should be empty.");

            var s = db.GetSchema(keyspaceName);

            // Get MetadataTable
            string metadataTableName = "evolve_change_log";
            var metadataTable = db.GetMetadataTable(keyspaceName, metadataTableName);

            // Create MetadataTable
            Assert.False(metadataTable.IsExists(), "MetadataTable sould not already exist.");
            Assert.True(metadataTable.CreateIfNotExists(), "MetadataTable creation failed.");
            Assert.True(metadataTable.IsExists(), "MetadataTable sould exist.");
            Assert.False(metadataTable.CreateIfNotExists(), "MetadataTable already exists. Creation should return false.");
            Assert.True(metadataTable.GetAllMigrationMetadata().Count() == 0, "No migration metadata should be found.");

            //Lock & Unlock
            //..Applicaiton level: return true if the cluster lock keyspace/table is not present
            Assert.True(db.TryAcquireApplicationLock());
            wcnn.ExecuteNonQuery("create keyspace cluster_lock with replication = { 'class' : 'SimpleStrategy','replication_factor' : '1' }; ");
            Assert.True(db.TryAcquireApplicationLock()); //Still true, table is missing
            wcnn.ExecuteNonQuery("create table cluster_lock.lock (locked int, primary key(locked))");
            Assert.True(db.TryAcquireApplicationLock()); //Still true, lock is not present
            wcnn.ExecuteNonQuery("insert into cluster_lock.lock (locked) values (1) using TTL 3600");
            Assert.False(db.TryAcquireApplicationLock());
            wcnn.ExecuteNonQuery("drop keyspace cluster_lock");
            Assert.True(db.TryAcquireApplicationLock());
            Assert.True(db.ReleaseApplicationLock());
            Assert.True(db.ReleaseApplicationLock());
            //..Table level: lock implemented with LWT on evolve metadata table
            Assert.True(metadataTable.TryLock());
            Assert.False(metadataTable.TryLock());
            Assert.True(metadataTable.ReleaseLock());
            Assert.False(metadataTable.ReleaseLock());

            // Save NewSchema metadata
            metadataTable.Save(MetadataType.NewSchema, "0", "New schema created.", keyspaceName);
            Assert.True(metadataTable.CanDropSchema(keyspaceName), $"[{keyspaceName}] should be droppable.");
            Assert.False(metadataTable.CanEraseSchema(keyspaceName), $"[{keyspaceName}] should not be erasable.");

            // Add metadata migration
            var migration = new FileMigrationScript(TestContext.Cassandra.EmptyMigrationScriptPath, "1_3_2", "desc");
            metadataTable.SaveMigration(migration, true);
            var migrationMetadata = metadataTable.GetAllMigrationMetadata().FirstOrDefault();
            Assert.True(migrationMetadata != null, "One migration metadata should be found.");
            Assert.True(migrationMetadata.Version == migration.Version, "Metadata version is not the same.");
            Assert.True(migrationMetadata.Checksum == migration.CalculateChecksum(), "Metadata checksum is not the same.");
            Assert.True(migrationMetadata.Description == migration.Description, "Metadata descritpion is not the same.");
            Assert.True(migrationMetadata.Name == migration.Name, "Metadata name is not the same.");
            Assert.True(migrationMetadata.Success == true, "Metadata success is not true.");
            Assert.True(migrationMetadata.Id != 0, "Metadata id is not set.");
            Assert.True(migrationMetadata.InstalledOn.Date == DateTime.UtcNow.Date, $"Installed date is {migrationMetadata.InstalledOn.Date} whereas UtcNow is {DateTime.UtcNow.Date}.");

            // Update checksum
            metadataTable.UpdateChecksum(migrationMetadata.Id, "Hi !");
            Assert.Equal("Hi !", metadataTable.GetAllMigrationMetadata().First().Checksum);

            // Assert metadata schema is not empty
            Assert.False(schema.IsEmpty(), $"[{keyspaceName}] should not be empty.");

            // Erase schema
            schema.Erase();
            Assert.True(schema.IsEmpty(), $"The schema [{keyspaceName}] should be empty.");
            Assert.True(schema.IsExists(), $"The schema [{keyspaceName}] should exist.");

            // Drop schema
            schema.Drop();
            Assert.False(schema.IsExists(), $"The schema [{keyspaceName}] should not exist.");
        }
    }
}
