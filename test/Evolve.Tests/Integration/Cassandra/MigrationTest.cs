using EvolveDb.Tests.Infrastructure;
using System.Collections.Generic;
using Xunit.Abstractions;
using static EvolveDb.Tests.TestContext;

namespace EvolveDb.Tests.Integration.Cassandra
{
    public record MigrationTest(ITestOutputHelper Output) : DbContainerFixture<CassandraContainer>
    {
        [FactSkippedOnAppVeyorOrLocal]
        [Category(Test.Cassandra)]
        public void Run_all_Cassandra_integration_tests_work()
        {
            // Arrange
            string metadataKeyspaceName = "my_keyspace_1"; // this name must also be declared in _evolve.cassandra.json
            var cnn = CreateDbConnection();
            var evolve = new Evolve(cnn, msg => Output.WriteLine(msg))
            {
                CommandTimeout = 25,
                MetadataTableSchema = metadataKeyspaceName,
                MetadataTableName = "evolve_change_log",
                Placeholders = new Dictionary<string, string> { ["${keyspace}"] = metadataKeyspaceName },
                SqlMigrationSuffix = ".cql"
            };
            evolve.Erase();

            // Assert
            evolve.AssertInfoIsSuccessful(cnn)
                  .ChangeLocations(CassandraDb.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(CassandraDb.ChecksumMismatchFolder)
                  .AssertMigrateThrows<EvolveValidationException>(cnn)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .ChangeLocations(CassandraDb.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations()
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(CassandraDb.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);
        }
    }
}
