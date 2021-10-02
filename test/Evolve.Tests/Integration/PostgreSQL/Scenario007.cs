using System.Collections.Generic;
using System.Linq;
using Evolve.Configuration;
using Evolve.Migration;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Integration.PostgreSql
{
    [Collection("PostgreSql collection")]
    public class Scenario007 : Scenario<PostgreSqlFixture>
    {
        public Scenario007(PostgreSqlFixture dbContainer, ITestOutputHelper output) : base(dbContainer, output) {}

        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Scenario_use_a_custom_migration_loader_only_when_is_set()
        {
            // Arrange
            Evolve.SqlRepeatableMigrationPrefix = "V";

            // Assert default configuration
            Assert.True(Evolve.MigrationLoader is FileMigrationLoader);
            Assert.Single(Evolve.MigrationLoader.GetMigrations());
            Assert.Single(Evolve.MigrationLoader.GetRepeatableMigrations());

            // Assert custom MigrationLoader returns no versioned migration
            Evolve.MigrationLoader = new CustomMigrationLoader(Evolve);

            Assert.True(Evolve.MigrationLoader is CustomMigrationLoader);
            Assert.Empty(Evolve.MigrationLoader.GetMigrations());
            Assert.Single(Evolve.MigrationLoader.GetRepeatableMigrations());
        }
    }

    internal class CustomMigrationLoader : FileMigrationLoader
    {
        public CustomMigrationLoader(IEvolveConfiguration options) : base(options) { }

        public override IEnumerable<MigrationScript> GetMigrations()
        {
            return Enumerable.Empty<MigrationScript>();
        }
    }
}
