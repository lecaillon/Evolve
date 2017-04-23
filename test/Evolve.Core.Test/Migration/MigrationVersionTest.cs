using System.Collections.Generic;
using System.Linq;
using Evolve.Migration;
using Xunit;

namespace Evolve.Core.Test.Migration
{
    public class MigrationVersionTest
    {
        [Theory(DisplayName = "Can_get_new_migration_version")]
        [InlineData("1_2_3_4")]
        [InlineData("1_2")]
        [InlineData("1_20_31_4000")]
        [InlineData("1")]
        public void Can_get_new_migration_version(string version)
        {
            Assert.Equal(new MigrationVersion(version).VersionParts, version.Split('_').Select(long.Parse).ToList());
        }

        [Theory(DisplayName = "When_version_format_is_incorrect_Throws_EvolveConfigurationException")]
        [InlineData("1_2_3__4")]
        [InlineData("1_2_3_4_")]
        [InlineData(".1.2.3.4")]
        [InlineData("1.2.3.a")]
        public void When_version_format_is_incorrect_Throws_EvolveConfigurationException(string version)
        {
            Assert.Throws<EvolveConfigurationException>(() => new MigrationVersion(version));
        }

        [Fact(DisplayName = "Versions_should_be_well_ordered")]
        public void Versions_should_be_well_ordered()
        {
            var list = new List<MigrationVersion>
            {
                new MigrationVersion("2"),      // 3
                new MigrationVersion("1.1"),    // 1
                new MigrationVersion("3.12.1"), // 8
                new MigrationVersion("1"),      // 0
                new MigrationVersion("1.1.0"),  // 2
                new MigrationVersion("2.1.0"),  // 4
                new MigrationVersion("3.11.2"), // 7
                new MigrationVersion("2.1.1"),  // 5
                new MigrationVersion("3.0"),    // 6
            };

            list.Sort();

            Assert.Equal("1", list[0].Label);
            Assert.Equal("1.1", list[1].Label);
            Assert.Equal("1.1.0", list[2].Label);
            Assert.Equal("2", list[3].Label);
            Assert.Equal("2.1.0", list[4].Label);
            Assert.Equal("2.1.1", list[5].Label);
            Assert.Equal("3.0", list[6].Label);
            Assert.Equal("3.11.2", list[7].Label);
            Assert.Equal("3.12.1", list[8].Label);
        }

        [Fact(DisplayName = "Version_comparaison_should_be_logical")]
        public void Version_comparaison_should_be_logical()
        {
            Assert.True(new MigrationVersion("1") == new MigrationVersion("1"));
            Assert.True(new MigrationVersion("1.1.1.12") == new MigrationVersion("1.1.1.12"));

            Assert.True(new MigrationVersion("1") != new MigrationVersion("1.0"));
            Assert.True(new MigrationVersion("1.1") != new MigrationVersion("1.10"));

            Assert.True(new MigrationVersion("1") < new MigrationVersion("2"));
            Assert.True(new MigrationVersion("1") < new MigrationVersion("1.0"));
            Assert.True(new MigrationVersion("1.1.1") < new MigrationVersion("1.1.2.0"));

            Assert.True(new MigrationVersion("2") > new MigrationVersion("1"));
            Assert.True(new MigrationVersion("1") > new MigrationVersion("0.5"));
            Assert.True(new MigrationVersion("1.1.1") > new MigrationVersion("1.0.9"));
        }
    }
}
