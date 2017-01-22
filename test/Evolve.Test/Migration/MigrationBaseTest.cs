using Evolve.Migration;
using System.Collections.Generic;
using Xunit;

namespace Evolve.Test.Migration
{
    public class MigrationBaseTest
    {
        [Fact]
        public void Migrations_should_be_well_ordered()
        {
            var list = new List<MigrationBase>
            {
                new MigrationMetadata("2", "desc", "name"),      // 3
                new MigrationMetadata("1.1", "desc", "name"),    // 1
                new MigrationMetadata("3.12.1", "desc", "name"), // 8
                new MigrationMetadata("1", "desc", "name"),      // 0
                new MigrationMetadata("1.1.0", "desc", "name"),  // 2
                new MigrationScript("2.1.0", "desc", "name"),  // 4
                new MigrationScript("3.11.2", "desc", "name"), // 7
                new MigrationScript("2.1.1", "desc", "name"),  // 5
                new MigrationScript("3.0", "desc", "name"),    // 6
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

        [Fact]
        public void Migration_comparaison_should_be_logical()
        {
            Assert.True(new MigrationMetadata("1", "desc", "name") == new MigrationScript("1", "desc", "name"));
            Assert.True(new MigrationMetadata("1.1.1.12", "desc", "name") == new MigrationScript("1.1.1.12", "desc", "name"));

            Assert.True(new MigrationMetadata("1", "desc", "name") != new MigrationMetadata("1.0", "desc", "name"));
            Assert.True(new MigrationScript("1.1", "desc", "name") != new MigrationScript("1.10", "desc", "name"));

            Assert.True(new MigrationMetadata("1", "desc", "name") < new MigrationMetadata("2", "desc", "name"));
            Assert.True(new MigrationMetadata("1", "desc", "name") < new MigrationScript("1.0", "desc", "name"));
            Assert.True(new MigrationScript("1.1.1", "desc", "name") < new MigrationScript("1.1.2.0", "desc", "name"));

            Assert.True(new MigrationMetadata("2", "desc", "name") > new MigrationMetadata("1", "desc", "name"));
            Assert.True(new MigrationMetadata("1", "desc", "name") > new MigrationScript("0.5", "desc", "name"));
            Assert.True(new MigrationScript("1.1.1", "desc", "name") > new MigrationScript("1.0.9", "desc", "name"));
        }
    }
}
