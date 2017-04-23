using Evolve.Utilities;
using Xunit;

namespace Evolve.Core.Test.Utilities
{
    public class MigrationUtilTest
    {
        [Theory(DisplayName = "Can_get_version_and_description")]
        [InlineData("V1__desc.sql", "1", "desc")]
        [InlineData("V1_3_1__Migration-desc.sql", "1_3_1", "Migration-desc")]
        public void Can_get_migration_version_and_description(string script, string expectedVersion, string expectedDescription)
        {
            MigrationUtil.ExtractVersionAndDescription(script, TestContext.SqlMigrationPrefix, TestContext.SqlMigrationSeparator, out string version, out string description);
            Assert.Equal(expectedVersion, version);
            Assert.Equal(expectedDescription, description);
        }

        [Theory(DisplayName = "When_migration_name_format_is_incorrect_Throws_EvolveConfigurationException")]
        [InlineData("V1_desc.sql")]
        [InlineData(@"C:\My__Folder\V1_desc.sql")]
        [InlineData("1_3_1__Migration-desc.sql")]
        [InlineData("V__Migration-desc.sql")]
        [InlineData("V1_3_1__.sql")]
        public void When_migration_name_format_is_incorrect_Throws_EvolveConfigurationException(string script)
        {
            Assert.Throws<EvolveConfigurationException>(() => MigrationUtil.ExtractVersionAndDescription(script, TestContext.SqlMigrationPrefix, TestContext.SqlMigrationSeparator, out string version, out string description));
        }
    }
}
