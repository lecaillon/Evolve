using Evolve.Configuration;
using Evolve.Migration;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Evolve.Test.Configuration
{
    public class AppConfigConfigurationProviderTest
    {
        [Fact(DisplayName = "Load_app_configuration_file_works")]
        public void Load_app_configuration_file_works()
        {
            var evolve = new Evolve();
            var configurationProvider = new AppConfigConfigurationProvider();
            configurationProvider.Configure(TestContext.AppConfigPath, evolve);

            Assert.Equal("Server=127.0.0.1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;", evolve.ConnectionString);
            Assert.Equal("npgsql", evolve.Driver);
            Assert.True(new List<string>() { "migration", "dataset" }.SequenceEqual(evolve.Locations));
            Assert.Equal("utf-16", evolve.Encoding.BodyName);
            Assert.Equal("Ver", evolve.SqlMigrationPrefix);
            Assert.Equal("@", evolve.SqlMigrationSeparator);
            Assert.Equal(".query", evolve.SqlMigrationSuffix);
            Assert.Equal("my_shema", evolve.DefaultSchema);
            Assert.Equal("my_metadata_schema", evolve.MetadaTableSchema);
            Assert.Equal("metadata_store", evolve.MetadaTableName);
            Assert.Equal("@{", evolve.PlaceholderPrefix);
            Assert.Equal("@}", evolve.PlaceholderSuffix);
            Assert.Equal(new MigrationVersion("2_1_0"), evolve.TargetVersion);
        }

        [Fact(DisplayName = "Load_web_configuration_file_works")]
        public void Load_web_configuration_file_works()
        {
            var expectedEvolve = new Evolve();
            var evolve = new Evolve();
            var configurationProvider = new AppConfigConfigurationProvider();
            configurationProvider.Configure(TestContext.WebConfigPath, evolve);

            Assert.Equal("Server=127.0.0.1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;", evolve.ConnectionString);
            Assert.Equal("npgsql", evolve.Driver);
            Assert.NotNull(evolve.Locations);
            Assert.Equal(expectedEvolve.Encoding.BodyName, evolve.Encoding.BodyName);
            Assert.Equal(expectedEvolve.SqlMigrationPrefix, evolve.SqlMigrationPrefix);
            Assert.Equal(expectedEvolve.SqlMigrationSeparator, evolve.SqlMigrationSeparator);
            Assert.Equal(expectedEvolve.SqlMigrationSuffix, evolve.SqlMigrationSuffix);
            Assert.Equal("my_shema", evolve.DefaultSchema);
            Assert.Equal(evolve.DefaultSchema, evolve.MetadaTableSchema);
            Assert.Equal(expectedEvolve.MetadaTableName, evolve.MetadaTableName);
            Assert.Equal(expectedEvolve.PlaceholderPrefix, evolve.PlaceholderPrefix);
            Assert.Equal(expectedEvolve.PlaceholderSuffix, evolve.PlaceholderSuffix);
            Assert.Equal(expectedEvolve.TargetVersion, evolve.TargetVersion);
        }
    }
}
