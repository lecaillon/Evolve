using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            // Assert default configuration
            Assert.True(Evolve.MigrationLoader is FileMigrationLoader);
            Assert.Single(Evolve.MigrationLoader.GetMigrations("V", "__", ".sql", Encoding.UTF8));
            Assert.Single(Evolve.MigrationLoader.GetRepeatableMigrations("V", "__", ".sql", Encoding.UTF8));

            // Assert custom MigrationLoader returns no versioned migration
            Evolve.MigrationLoader = new CustomMigrationLoader(new[] { ScenarioFolder });

            Assert.True(Evolve.MigrationLoader is CustomMigrationLoader);
            Assert.Empty(Evolve.MigrationLoader.GetMigrations("V", "__", ".sql", Encoding.UTF8));
            Assert.Single(Evolve.MigrationLoader.GetRepeatableMigrations("V", "__", ".sql", Encoding.UTF8));
        }
    }

    internal class CustomMigrationLoader : FileMigrationLoader
    {
        public CustomMigrationLoader(IEnumerable<string> locations) : base(locations)
        {
        }

        public override IEnumerable<MigrationScript> GetMigrations(string prefix, string separator, string suffix, Encoding encoding = null)
        {
            return Enumerable.Empty<MigrationScript>();
        }
    }
}
