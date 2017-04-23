using System.Collections.Generic;
using Evolve.Metadata;
using Evolve.Migration;
using Xunit;

namespace Evolve.Core.Test.Migration
{
    public class MigrationBaseTest
    {
        [Fact(DisplayName = "Migrations_should_be_well_ordered")]
        public void Migrations_should_be_well_ordered()
        {
            var list = new List<MigrationBase>
            {
                new MigrationMetadata("2", "desc", "name", MetadataType.Migration),      // 3
                new MigrationMetadata("1.1", "desc", "name", MetadataType.Migration),    // 1
                new MigrationMetadata("3.12.1", "desc", "name", MetadataType.Migration), // 8
                new MigrationMetadata("1", "desc", "name", MetadataType.Migration),      // 0
                new MigrationMetadata("1.1.0", "desc", "name", MetadataType.Migration),  // 2
                new MigrationScript(TestContext.ValidMigrationScriptPath, "2.1.0", "desc"),  // 4
                new MigrationScript(TestContext.ValidMigrationScriptPath, "3.11.2", "desc"), // 7
                new MigrationScript(TestContext.ValidMigrationScriptPath, "2.1.1", "desc"),  // 5
                new MigrationScript(TestContext.ValidMigrationScriptPath, "3.0", "desc"),    // 6
            };

            list.Sort();

            Assert.Equal("1", list[0].Version.Label);
            Assert.Equal("1.1", list[1].Version.Label);
            Assert.Equal("1.1.0", list[2].Version.Label);
            Assert.Equal("2", list[3].Version.Label);
            Assert.Equal("2.1.0", list[4].Version.Label);
            Assert.Equal("2.1.1", list[5].Version.Label);
            Assert.Equal("3.0", list[6].Version.Label);
            Assert.Equal("3.11.2", list[7].Version.Label);
            Assert.Equal("3.12.1", list[8].Version.Label);
        }

        [Fact(DisplayName = "Migration_comparaison_should_be_logical")]
        public void Migration_comparaison_should_be_logical()
        {
            Assert.True(new MigrationMetadata("1", "desc", "name", MetadataType.Migration) == new MigrationScript(TestContext.ValidMigrationScriptPath, "1", "desc"));
            Assert.True(new MigrationMetadata("1.1.1.12", "desc", "name", MetadataType.Migration) == new MigrationScript(TestContext.ValidMigrationScriptPath, "1.1.1.12", "desc"));

            Assert.True(new MigrationMetadata("1", "desc", "name", MetadataType.Migration) != new MigrationMetadata("1.0", "desc", "name", MetadataType.Migration));
            Assert.True(new MigrationScript(TestContext.ValidMigrationScriptPath, "1.1", "desc") != new MigrationScript(TestContext.ValidMigrationScriptPath, "1.10", "desc"));

            Assert.True(new MigrationMetadata("1", "desc", "name", MetadataType.Migration) < new MigrationMetadata("2", "desc", "name", MetadataType.Migration));
            Assert.True(new MigrationMetadata("1", "desc", "name", MetadataType.Migration) < new MigrationScript(TestContext.ValidMigrationScriptPath, "1.0", "desc"));
            Assert.True(new MigrationScript(TestContext.ValidMigrationScriptPath, "1.1.1", "desc") < new MigrationScript(TestContext.ValidMigrationScriptPath, "1.1.2.0", "desc"));

            Assert.True(new MigrationMetadata("2", "desc", "name", MetadataType.Migration) > new MigrationMetadata("1", "desc", "name", MetadataType.Migration));
            Assert.True(new MigrationMetadata("1", "desc", "name", MetadataType.Migration) > new MigrationScript(TestContext.ValidMigrationScriptPath, "0.5", "desc"));
            Assert.True(new MigrationScript(TestContext.ValidMigrationScriptPath, "1.1.1", "desc") > new MigrationScript(TestContext.ValidMigrationScriptPath, "1.0.9", "desc"));
        }
    }
}
