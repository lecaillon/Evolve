using System.Collections.Generic;
using System.Linq;
using Evolve.Migration;
using Xunit;

namespace Evolve.Test.Migration
{
    [Collection("Embedded resource migrations")]
    public class EmbeddedResourceMigrationLoaderTest
    {
        [Fact]
        public void LoadEmbeddedResource()
        {
            IList<IMigrationScript> scripts = new EmbeddedResourceMigrationLoader(GetType().Assembly).GetMigrations(new List<string> { "Evolve.Test.Resources.scripts." }, "V",
                "__", ".sql").ToList();
            Assert.Equal(1,scripts.Count);
            var script = scripts.First();
            Assert.Equal("V1_0_0__Test-Migration.sql", script.Name);
            Assert.NotNull(script.CheckSum);
            Assert.Equal("select 1;", script.LoadSqlStatements(null,"GO").FirstOrDefault());
        }

        [Fact]
        public void FailOnDuplicate()
        {
            Assert.Throws<EvolveConfigurationException>(() =>
                new EmbeddedResourceMigrationLoader(GetType().Assembly)
                    .GetMigrations(new List<string> { "Evolve.Test.Resources.scripts", "Evolve.Test.Resources.scripts2" },
                        "V", "__", ".sql").ToList());
        }
    }
}