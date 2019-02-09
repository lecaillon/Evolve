using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Evolve.Migration;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static Evolve.Dialect.Cassandra.Configuration;

namespace Evolve.Tests.Integration.Cassandra
{
    [Collection("Cassandra collection")]
    public class MigrationTest
    {
        private readonly CassandraFixture _cassandraContainer;
        private readonly ITestOutputHelper _output;


        public MigrationTest(CassandraFixture cassandraContainer, ITestOutputHelper output)
        {
            _cassandraContainer = cassandraContainer;
            _output = output;

            if (TestContext.Local)
            {
                cassandraContainer.Run(fromScratch: true);
            }
        }

        [FactSkippedOnAppVeyor]
        [Category(Test.Cassandra)]
        public void Run_all_Cassandra_integration_tests_work()
        {
            string metadataKeyspaceName = "my_keyspace_1"; // this name must also be declared in _evolve.cassandra.json
            var cnn = _cassandraContainer.CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Locations = new List<string> { TestContext.Cassandra.MigrationFolder },
                CommandTimeout = 25,
                MetadataTableSchema = metadataKeyspaceName,
                MetadataTableName = "evolve_change_log",
                Placeholders = new Dictionary<string, string> { ["${keyspace}"] = metadataKeyspaceName },
                SqlMigrationSuffix = ".cql"
            };

            int nbMigration = Directory.GetFiles(TestContext.Cassandra.MigrationFolder).Length;

            // Migrate Cql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            Assert.Equal(ConnectionState.Closed, cnn.State);

            // Migrate: nothing to do. Database is already up to date.
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration}.");
            Assert.Equal(ConnectionState.Closed, cnn.State);

            // StartVersion fails because migraiton scripts has been applied.
            evolve.StartVersion = new MigrationVersion("3.0");
            Assert.Throws<EvolveConfigurationException>(() => evolve.Migrate());
            evolve.StartVersion = MigrationVersion.MinVersion;
            Assert.Equal(ConnectionState.Closed, cnn.State);

            // Migrate Cql_Scripts\Checksum_mismatch: validation should fail due to a checksum mismatch.
            evolve.Locations = new List<string> { TestContext.Cassandra.ChecksumMismatchFolder };
            Assert.Throws<EvolveValidationException>(() => evolve.Migrate());
            Assert.Equal(ConnectionState.Closed, cnn.State);

            // Repair sucessfull
            evolve.Repair();
            Assert.True(evolve.NbReparation == 1, $"There should be 1 migration repaired, not {evolve.NbReparation}.");
            Assert.Equal(ConnectionState.Closed, cnn.State);

            // Migrate: nothing to do. Database is already up to date.
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration}.");
            Assert.Equal(ConnectionState.Closed, cnn.State);

            // Erase cancelled (EraseDisabled = true)
            evolve.IsEraseDisabled = true;
            Assert.Throws<EvolveConfigurationException>(() => evolve.Erase());
            Assert.Equal(ConnectionState.Closed, cnn.State);

            // Erase sucessfull
            evolve.IsEraseDisabled = false;
            evolve.Erase();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");
            Assert.Equal(ConnectionState.Closed, cnn.State);

            // Migrate sucessfull after a validation error (MustEraseOnValidationError = true)
            evolve.Locations = new List<string> { TestContext.Cassandra.MigrationFolder }; // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            evolve.Locations = new List<string> { TestContext.Cassandra.ChecksumMismatchFolder }; // Migrate Cql_Scripts\Checksum_mismatch
            evolve.MustEraseOnValidationError = true;
            evolve.Migrate();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");
            Assert.True(evolve.NbMigration == 1, $"1 migration should have been applied, not {evolve.NbMigration}.");
            Assert.Equal(ConnectionState.Closed, cnn.State);

            // Erase sucessfull
            evolve.IsEraseDisabled = false;
            evolve.Erase();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");
            Assert.Equal(ConnectionState.Closed, cnn.State);

            // StartVersion = 3
            evolve.Erase();
            evolve.Locations = new List<string> { TestContext.Cassandra.MigrationFolder }; // Migrate Cql_Scripts\Migration
            evolve.StartVersion = new MigrationVersion("3");
            evolve.Migrate();
            Assert.True(evolve.NbMigration == (nbMigration - 2), $"{nbMigration - 2} migrations should have been applied, not {evolve.NbMigration} (StartVersion tests).");
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration} (StartVersion tests).");
            evolve.StartVersion = MigrationVersion.MinVersion;
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration} (StartVersion tests).");
            evolve.StartVersion = new MigrationVersion("3.0");
            Assert.Throws<EvolveConfigurationException>(() => evolve.Migrate());
            Assert.Equal(ConnectionState.Closed, cnn.State);

            //DefaultKeyspaceReplicationStrategy
            evolve.StartVersion = MigrationVersion.MinVersion;
            evolve.Erase();
            var configurationFileName = ConfigurationFile;
            File.Copy($"_{configurationFileName}", configurationFileName);
            var ex = Assert.Throws<EvolveSqlException>(() => evolve.Migrate());
            Assert.Contains("Not enough replicas available for query at consistency", ex.Message);
            File.Delete(configurationFileName);

            // Call the second part of the Cassandra integration tests
            DialectTest.Run_all_Cassandra_integration_tests_work(_cassandraContainer);
        }
    }
}
