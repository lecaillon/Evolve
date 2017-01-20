using Evolve.Migration;
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

            Assert.Equal(new MigrationVersion(version1).VersionParts, version1.Split('_').Select(int.Parse).ToList());
            Assert.Equal(new MigrationVersion(version2).VersionParts, version2.Split('_').Select(int.Parse).ToList());
            Assert.Equal(new MigrationVersion(version3).VersionParts, version3.Split('.').Select(int.Parse).ToList());
            Assert.Equal(new MigrationVersion(version4).VersionParts, version4.Split('_').Select(int.Parse).ToList());
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
    }
}
