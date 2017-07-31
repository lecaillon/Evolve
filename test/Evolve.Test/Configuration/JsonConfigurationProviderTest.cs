using System.Collections.Generic;
using System.Linq;
using Evolve.Configuration;
using Evolve.Migration;
using Xunit;

namespace Evolve.Test.Configuration
{
    public class JsonConfigurationProviderTest
    {
        [Fact(DisplayName = "Load_json_configuration_file_works")]
        public void Load_json_configuration_file_works()
        {
            var evolve = new Evolve(TestContext.JsonConfigPath);

            Assert.Equal("Server=127.0.0.1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;", evolve.ConnectionString);
            Assert.Equal("npgsql", evolve.Driver);
            Assert.True(new List<string>() { "migration", "dataset" }.SequenceEqual(evolve.Locations));
            Assert.Equal("utf-16", evolve.Encoding.BodyName);
            Assert.Equal("Ver", evolve.SqlMigrationPrefix);
            Assert.Equal("@", evolve.SqlMigrationSeparator);
            Assert.Equal(".query", evolve.SqlMigrationSuffix);
            Assert.Equal("my_shema", evolve.Schemas.Single());
            Assert.Equal("my_metadata_schema", evolve.MetadataTableSchema);
            Assert.Equal("metadata_store", evolve.MetadataTableName);
            Assert.Equal("@{", evolve.PlaceholderPrefix);
            Assert.Equal("@}", evolve.PlaceholderSuffix);
            Assert.Equal(new MigrationVersion("2_1_0"), evolve.TargetVersion);
            Assert.Equal(new MigrationVersion("1_1_0"), evolve.StartVersion);
            Assert.True(new List<string>() { "@{Pwd@}", "@{Schema@}" }.SequenceEqual(evolve.Placeholders.Keys.OrderBy(x => x)));
            Assert.True(new List<string>() { "my_schema", "password" }.SequenceEqual(evolve.Placeholders.Values.OrderBy(x => x)));
            Assert.True(evolve.IsEraseDisabled);
            Assert.True(evolve.MustEraseOnValidationError);
            Assert.Equal(evolve.Command, CommandOptions.Erase);
        }

        [Fact(DisplayName = "Load_multiple_json_configuration_files_works")]
        public void Load_multiple_json_configuration_files_works()
        {
            var evolve = new Evolve(TestContext.Json2ConfigPath, environmentName: "staging");

            Assert.Equal("Server=srv1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;", evolve.ConnectionString);
            Assert.Equal("npgsql", evolve.Driver);
            Assert.True(new List<string>() { "migration" }.SequenceEqual(evolve.Locations));
            Assert.Equal("utf-16", evolve.Encoding.BodyName);
            Assert.Equal("Ver", evolve.SqlMigrationPrefix);
            Assert.Equal("@", evolve.SqlMigrationSeparator);
            Assert.Equal(".query", evolve.SqlMigrationSuffix);
            Assert.Equal("my_shema", evolve.Schemas.Single());
            Assert.Equal("my_metadata_schema", evolve.MetadataTableSchema);
            Assert.Equal("metadata_store", evolve.MetadataTableName);
            Assert.Equal("@{", evolve.PlaceholderPrefix);
            Assert.Equal("@}", evolve.PlaceholderSuffix);
            Assert.Equal(new MigrationVersion("2_1_0"), evolve.TargetVersion);
            Assert.Equal(new MigrationVersion("1_1_0"), evolve.StartVersion);
            Assert.True(new List<string>() { "@{Pwd@}", "@{Schema@}" }.SequenceEqual(evolve.Placeholders.Keys.OrderBy(x => x)));
            Assert.True(new List<string>() { "my_schema", "password" }.SequenceEqual(evolve.Placeholders.Values.OrderBy(x => x)));
            Assert.True(evolve.IsEraseDisabled);
            Assert.False(evolve.MustEraseOnValidationError);
            Assert.Equal(evolve.Command, CommandOptions.Migrate);
        }
    }
}
