using System.Linq;
using Evolve.Migration;
using Xunit;

namespace Evolve.Tests.Migration
{
    public class EmbeddedResourceMigrationLoaderTest
    {
        [Fact]
        [Category(Test.Migration)]
        public void Load_embedded_resource_migrations_works()
        {
            // Arrange
            var loader = new EmbeddedResourceMigrationLoader(
                assemblies: new[] { typeof(TestContext).Assembly }, 
                filters: new[] { "Evolve.Tests.Resources.Scripts_1" });

            // Act
            var scripts = loader.GetMigrations("V", "__", ".sql").ToList();

            // Assert
            Assert.Equal("1.3.0", scripts[0].Version.Label);
            Assert.Equal("1.3.1", scripts[1].Version.Label);
            Assert.Equal("1.5.0", scripts[2].Version.Label);
            Assert.Equal("2.0.0", scripts[3].Version.Label);
        }

        [Fact]
        [Category(Test.Migration)]
        public void When_duplicate_version_found_Throws_EvolveException()
        {
            var loader = new EmbeddedResourceMigrationLoader(assemblies: new[] { typeof(TestContext).Assembly }, filters: null);
            Assert.Throws<EvolveConfigurationException>(() => loader.GetMigrations("V", "__", ".sql"));
        }
    }
}
