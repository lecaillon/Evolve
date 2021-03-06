using System.Linq;
using Evolve.Configuration;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Integration.SQLServer
{
    [Collection("SQLServer collection")]
    public class Scenario101 : Scenario<SQLServerFixture>
    {
        public Scenario101(SQLServerFixture dbContainer, ITestOutputHelper output) : base(dbContainer, output) { }

        [Fact]
        [Category(Test.SQLServer, Test.Sceanario)]
        public void Scenario_sqlserver_tx_commit_all_only_when_every_scripts_succeed()
        {
            // Arrange
            Evolve.TransactionMode = TransactionKind.CommitAll;
            // Assert fails
            Evolve.AssertMigrateThrows<EvolveException>(Cnn);
            Assert.True(Evolve.AppliedMigrations.Count == 0, $"There should be no migration applied when a migration fails in a CommitAll transaction mode.");
            Assert.False(MetadataTable.GetAllAppliedMigration().Any(), $"There should be no versioned migration applied when a migration fails in a CommitAll transaction mode.");
            Assert.False(MetadataTable.GetAllAppliedRepeatableMigration().Any(), $"There should be no repeatable migration applied when a migration fails in a CommitAll transaction mode.");

            // Arrange
            Evolve.TargetVersion = new("2_0");
            // Assert succeeds
            Evolve.AssertMigrateIsSuccessful(Cnn);
        }
    }
}
