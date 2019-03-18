﻿using System.Collections.Generic;
using System.IO;
using Evolve.Migration;
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
            int expectedNbMigration = Directory.GetFiles(CassandraDb.MigrationFolder).Length;
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
            evolve.AssertMigrateIsSuccessful(cnn, expectedNbMigration, locations: CassandraDb.MigrationFolder)
                  .AssertMigrateThrows<EvolveConfigurationException>(cnn, e => e.StartVersion = new MigrationVersion("3.0"))
                  .AssertMigrateThrows<EvolveValidationException>(cnn, e => e.StartVersion = MigrationVersion.MinVersion, CassandraDb.ChecksumMismatchFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0)
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration, locations: CassandraDb.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 1, e => e.MustEraseOnValidationError = true, CassandraDb.ChecksumMismatchFolder)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration - 2, e => e.StartVersion = new MigrationVersion("3"), CassandraDb.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0, e => e.StartVersion = MigrationVersion.MinVersion)
                  .AssertMigrateThrows<EvolveConfigurationException>(cnn, e => e.StartVersion = new MigrationVersion("3.0"))
                  .AssertEraseIsSuccessful(cnn, e => e.StartVersion = MigrationVersion.MinVersion)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration, null, locations: CassandraDb.MigrationFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 0, locations: CassandraDb.RepeatableFolder)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 1)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0)
                  .AssertEraseIsSuccessful(cnn, e => e.StartVersion = MigrationVersion.MinVersion);

            //DefaultKeyspaceReplicationStrategy
            evolve.Locations = new[] { CassandraDb.MigrationFolder };
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
