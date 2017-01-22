using Evolve.Migration;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Evolve.Test.Migration
{
    public class MigrationVersionTest
    {
        [Fact]
        public void Can_get_new_migration_version()
        {
            string version1 = "1_2_3_4";
            string version2 = "1_2";
            string version3 = "1.20.31.4000";
            string version4 = "1";

            Assert.Equal(new MigrationVersion(version1).VersionParts, version1.Split('_').Select(long.Parse).ToList());
            Assert.Equal(new MigrationVersion(version2).VersionParts, version2.Split('_').Select(long.Parse).ToList());
            Assert.Equal(new MigrationVersion(version3).VersionParts, version3.Split('.').Select(long.Parse).ToList());
            Assert.Equal(new MigrationVersion(version4).VersionParts, version4.Split('_').Select(long.Parse).ToList());
        }

        [Fact]
        public void When_version_format_is_incorrect_Throws_EvolveConfigurationException()
        {
            string version1 = "1_2_3__4";
            string version2 = "1_2_3_4_";
            string version3 = ".1.2.3.4";
            string version4 = "1.2.3.a";

            Assert.Throws<EvolveConfigurationException>(() => new MigrationVersion(version1));
            Assert.Throws<EvolveConfigurationException>(() => new MigrationVersion(version2));
            Assert.Throws<EvolveConfigurationException>(() => new MigrationVersion(version3));
            Assert.Throws<EvolveConfigurationException>(() => new MigrationVersion(version4));
        }

        [Fact]
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

        [Fact]
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
