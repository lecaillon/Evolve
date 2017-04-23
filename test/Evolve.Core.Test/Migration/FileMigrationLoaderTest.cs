using System.Collections.Generic;
using System.Linq;
using Evolve.Migration;
using Xunit;

namespace Evolve.Core.Test.Migration
{
    public class FileMigrationLoaderTest
    {
        [Fact(DisplayName = "GetMigrations_works")]
        public void GetMigrations_works()
        {
            var loader = new FileMigrationLoader();
            var scripts = loader.GetMigrations(new List<string> { TestContext.ScriptsSQL1, TestContext.ScriptsSQL2, TestContext.ScriptsSQL1, TestContext.ScriptsSQL2 + @"\PSG" }, 
                                               TestContext.SqlMigrationPrefix, 
                                               TestContext.SqlMigrationSeparator, 
                                               TestContext.SqlMigrationSuffix).ToList();

            Assert.Equal("1.3.0", scripts[0].Version.Label);
            Assert.Equal("1.3.1", scripts[1].Version.Label);
            Assert.Equal("1.4.0", scripts[2].Version.Label);
            Assert.Equal("1.5.0", scripts[3].Version.Label);
            Assert.Equal("2.0.0", scripts[4].Version.Label);
            Assert.Equal("2.4.0", scripts[5].Version.Label);
        }

        [Fact(DisplayName = "When_duplicate_version_found_Throws_EvolveException")]
        public void When_duplicate_version_found_Throws_EvolveException()
        {
            var loader = new FileMigrationLoader();
            Assert.Throws<EvolveConfigurationException>(() => loader.GetMigrations(new List<string> { TestContext.ResourcesFolder },
                                                                                   TestContext.SqlMigrationPrefix,
                                                                                   TestContext.SqlMigrationSeparator,
                                                                                   TestContext.SqlMigrationSuffix));
        }
    }
}
