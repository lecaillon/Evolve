using System.Linq;
using Evolve.Configuration;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Integration.SQLServer
{
    [Collection("SQLServer collection")]
    public class Scenario102 : Scenario<SQLServerFixture>
    {
        public Scenario102(SQLServerFixture dbContainer, ITestOutputHelper output) : base(dbContainer, output) { }

        [Fact]
        [Category(Test.SQLServer, Test.Sceanario)]
        public void Scenario_sqlserver_tx_rollback_all_either_when_migration_succeeds_or_fails()
        {
            // Arrange
            Evolve.TransactionMode = TransactionKind.RollbackAll;
            // Assert migration fails on last script, nothing applied
            Evolve.AssertMigrateThrows<EvolveException>(Cnn);
            Assert.True(Evolve.AppliedMigrations.Count == 0, $"There should be no migration applied when a migration fails in RollbackAll mode.");
            Assert.False(MetadataTable.GetAllAppliedMigration().Any());
            Assert.False(MetadataTable.GetAllAppliedRepeatableMigration().Any());

            // Arrange
            Evolve.TargetVersion = new("2_0");
            // Assert migration succeeds, nothing applied
            Evolve.Migrate();
            Assert.True(Evolve.AppliedMigrations.Count == 0, $"There should be no migration applied when a migration succeeds in RollbackAll mode.");
            Assert.False(MetadataTable.GetAllAppliedMigration().Any());
            Assert.False(MetadataTable.GetAllAppliedRepeatableMigration().Any());
        }
    }
}
