using Cassandra.Data;
using Evolve.Test.Utilities;
using System.Collections.Generic;
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

            // Migrate Cql_Scripts\Migration
            evolve.Migrate();
            Assert.Equal(2, evolve.NbMigration);
        }
    }
}
