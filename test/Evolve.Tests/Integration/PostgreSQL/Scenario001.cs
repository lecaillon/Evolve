using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.PostgreSql
{
    [Collection("PostgreSql collection")]
    public class Scenario001
    {
        private readonly PostgreSqlFixture _dbContainer;
        private readonly ITestOutputHelper _output;

        public Scenario001(PostgreSqlFixture dbContainer, ITestOutputHelper output)
        {
            _dbContainer = dbContainer;
            _output = output;

            if (Local)
            {
                dbContainer.Run(fromScratch: true);
            }
        }

        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Sceanrio_retry_repeatable_migrations_until_no_error()
        {
            // Arrange
            var cnn = _dbContainer.CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Schemas = new[] { "scenario001" },
                MetadataTableSchema = "scenario001",
                Locations = new[] { PostgreSQL.Scenario001Folder },
                RetryRepeatableMigrationsUntilNoError = false
            };

            // Act
            evolve.AssertInfoIsSuccessfulV2(cnn);

        }
    }
}
