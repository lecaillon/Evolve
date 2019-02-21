using System.Linq;
using Evolve.Migration;
using Xunit;

namespace Evolve.Tests.Migration
{
    public class FileMigrationLoaderTest
    {
        [Fact]
        [Category(Test.Migration)]
        public void Load_file_migrations_works()
        {
            var loader = new FileMigrationLoader(new[]
            {
                TestContext.Scripts1,
                TestContext.Scripts2,
                TestContext.Scripts1,
                TestContext.Scripts2 + "/PSG"
            });
            var scripts = loader.GetMigrations("V", "__", ".sql").ToList();

            Assert.Equal("1.3.0", scripts[0].Version.Label);
            Assert.Equal("1.3.1", scripts[1].Version.Label);
            Assert.Equal("1.4.0", scripts[2].Version.Label);
            Assert.Equal("1.5.0", scripts[3].Version.Label);
            Assert.Equal("2.0.0", scripts[4].Version.Label);
            Assert.Equal("2.4.0", scripts[5].Version.Label);
        }

        [Fact]
        [Category(Test.Migration)]
        public void When_duplicate_version_found_Throws_EvolveException()
        {
            var loader = new FileMigrationLoader(new[] { TestContext.ResourcesFolder });
            Assert.Throws<EvolveConfigurationException>(() => loader.GetMigrations("V", "__", ".sql"));
        }
    }
}
