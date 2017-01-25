using Evolve.Utilities;
using Xunit;

namespace Evolve.Test.Utilities
{
    public class MigrationUtilTest
    {
        [Theory(DisplayName = "Can_get_version_and_description")]
        [InlineData("V1__desc.sql", "1", "desc")]
        [InlineData("V1_3_1__Migration-desc.sql", "1_3_1", "Migration-desc")]
        [InlineData(@"C:\MyFolder\V1_12_1_0__migration_description.sql", "1_12_1_0", "migration description")]
        public void Can_get_migration_version_and_description(string script, string expectedVersion, string expectedDescription)
        {
            var versionAndDescription = MigrationUtil.ExtractVersionAndDescription(script, TestContext.SqlMigrationPrefix, TestContext.SqlMigrationSeparator);
            Assert.Equal(expectedVersion, versionAndDescription.Item1);
            Assert.Equal(expectedDescription, versionAndDescription.Item2);
        }

        [Theory(DisplayName = "When_migration_name_format_is_incorrect_Throws_EvolveConfigurationException")]
        [InlineData("V1_desc.sql")]
        [InlineData(@"C:\My__Folder\V1_desc.sql")]
        [InlineData("1_3_1__Migration-desc.sql")]
        [InlineData("V__Migration-desc.sql")]
        [InlineData("V1_3_1__.sql")]
        public void When_migration_name_format_is_incorrect_Throws_EvolveConfigurationException(string script)
        {
            Assert.Throws<EvolveConfigurationException>(() => MigrationUtil.ExtractVersionAndDescription(script, TestContext.SqlMigrationPrefix, TestContext.SqlMigrationSeparator));
        }
    }
}
