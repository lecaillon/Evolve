using System.Collections.Generic;
using System.IO;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static Evolve.Dialect.Cassandra.Configuration;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.Cassandra
{
    [Collection("Cassandra collection")]
    public class MigrationTest
    {
        private readonly CassandraFixture _dbContainer;
        private readonly ITestOutputHelper _output;

        public MigrationTest(CassandraFixture dbContainer, ITestOutputHelper output)
        {
            _dbContainer = dbContainer;
            _output = output;

            if (Local)
            {
                dbContainer.Run(fromScratch: true);
            }
        }

        [FactSkippedOnAppVeyor]
        [Category(Test.Cassandra)]
        public void Run_all_Cassandra_integration_tests_work()
        {
            // Arrange
            string metadataKeyspaceName = "my_keyspace_1"; // this name must also be declared in _evolve.cassandra.json
            var cnn = _dbContainer.CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                CommandTimeout = 25,
                MetadataTableSchema = metadataKeyspaceName,
                MetadataTableName = "evolve_change_log",
                Placeholders = new Dictionary<string, string> { ["${keyspace}"] = metadataKeyspaceName },
                SqlMigrationSuffix = ".cql"
            };

            // Assert
            evolve.AssertInfoIsSuccessfulV2(cnn)
                  .ChangeLocations(CassandraDb.MigrationFolder)
                  .AssertInfoIsSuccessfulV2(cnn)
                  .AssertMigrateIsSuccessfulV2(cnn)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(CassandraDb.ChecksumMismatchFolder)
                  .AssertMigrateThrows<EvolveValidationException>(cnn)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .ChangeLocations(CassandraDb.MigrationFolder)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations()
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(CassandraDb.MigrationFolder)
                  .AssertMigrateIsSuccessfulV2(cnn)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false);

            //DefaultKeyspaceReplicationStrategy
            var configurationFileName = ConfigurationFile;
            File.Copy($"_{configurationFileName}", configurationFileName);
            var ex = Assert.Throws<EvolveSqlException>(() => evolve.Migrate());
            Assert.Contains("Not enough replicas available for query at consistency", ex.Message);
            File.Delete(configurationFileName);

            // Call the second part of the Cassandra integration tests
            DialectTest.Run_all_Cassandra_integration_tests_work(_dbContainer);
        }
    }
}
