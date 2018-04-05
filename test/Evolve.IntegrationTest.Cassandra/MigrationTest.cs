using Cassandra.Data;
using Evolve.Dialect.Cassandra;
using Evolve.Migration;
using Evolve.Test.Utilities;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.IntegrationTest.Cassandra
{
    [Collection("Database collection")]
    public class MigrationTest
    {
        private readonly CassandraFixture _cassandraFixture;
        private readonly ITestOutputHelper _output;

        public MigrationTest(CassandraFixture cassandraFixture, ITestOutputHelper output)
        {
            _cassandraFixture = cassandraFixture;
            _output = output;
            if (!TestContext.Travis && !TestContext.AppVeyor)
            { // AppVeyor and Windows 2016 does not support linux docker images
                cassandraFixture.Start(fromScratch: true);
            }
        }

        [SkipOnAppVeyorFact(DisplayName = "Run_all_Cassandra_migrations_work")]
        public void Run_all_Cassandra_migrations_work()
        {
            var cnn = new CqlConnection($"Contact Points=127.0.0.1;Port={_cassandraFixture.Cassandra.HostPort};Cluster Name={_cassandraFixture.Cassandra.ClusterName}");

            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Locations = new List<string> { TestContext.MigrationFolder },
                CommandTimeout = 25,
                MetadataTableSchema = "my_keyspace",
                MetadataTableName = "evolve_change_log",
                SqlMigrationSuffix = ".cql"
            };

            int nbMigration = Directory.GetFiles(TestContext.MigrationFolder).Length;

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
            evolve.Locations = new List<string> { TestContext.ChecksumMismatchFolder };
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
            evolve.Locations = new List<string> { TestContext.MigrationFolder }; // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            evolve.Locations = new List<string> { TestContext.ChecksumMismatchFolder }; // Migrate Cql_Scripts\Checksum_mismatch
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
            evolve.Locations = new List<string> { TestContext.MigrationFolder }; // Migrate Cql_Scripts\Migration
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
            File.Copy($"_{CassandraKeyspace.DefaultReplicationStrategyFile}", CassandraKeyspace.DefaultReplicationStrategyFile);
            var ex = Assert.Throws<EvolveSqlException>(() => evolve.Migrate());
            Assert.Contains("Not enough replicas available for query at consistency", ex.Message);
            File.Delete(CassandraKeyspace.DefaultReplicationStrategyFile);
        }
    }
}
