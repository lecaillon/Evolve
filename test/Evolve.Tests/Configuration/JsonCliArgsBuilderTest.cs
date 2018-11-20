using System;
using System.Collections.Generic;
using System.Linq;
using Evolve.MSBuild;
using Xunit;

namespace Evolve.Tests.Configuration
{
    public class JsonCliArgsBuilderTest
    {
        [Fact]
        public void Load_json_configuration_file_works()
        {
            var builder = new JsonCliArgsBuilder(TestContext.EvolveJsonPath);

            Assert.Equal("Erase", builder.Command);
            Assert.Equal("postgresql", builder.Database);
            Assert.Equal("Server=127.0.0.1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;", builder.ConnectionString);
            Assert.True(new List<string>() { "migration", "dataset" }.SequenceEqual(builder.Locations));
            Assert.Equal("true", builder.EraseDisabled);
            Assert.Equal("True", builder.EraseOnValidationError);
            Assert.Equal("utf-16", builder.Encoding);
            Assert.Equal("Ver", builder.SqlMigrationPrefix);
            Assert.Equal("@", builder.SqlMigrationSeparator);
            Assert.Equal(".query", builder.SqlMigrationSuffix);
            Assert.Equal("my_shema", builder.Schemas.Single());
            Assert.Equal("my_metadata_schema", builder.MetadataTableSchema);
            Assert.Equal("metadata_store", builder.MetadataTableName);
            Assert.Equal("@{", builder.PlaceholderPrefix);
            Assert.Equal("@}", builder.PlaceholderSuffix);
            Assert.Equal("2_1_0", builder.TargetVersion);
            Assert.Equal("1_1_0", builder.StartVersion);
            Assert.True(new List<string>() { "Schema:my_schema", "Pwd:password" }.SequenceEqual(builder.Placeholders));
            Assert.Equal("true", builder.EnableClusterMode);
            Assert.Equal("true", builder.OutOfOrder);
            Assert.Equal("200", builder.CommandTimeout);
        }

        [Fact]
        public void Load_multiple_json_configuration_files_works()
        {
            var builder = new JsonCliArgsBuilder(TestContext.Evolve2JsonPath, env: "staging");

            Assert.Equal("Server=srv1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;", builder.ConnectionString);
            Assert.Equal("postgresql", builder.Database);
            Assert.True(new List<string>() { "migration" }.SequenceEqual(builder.Locations));
            Assert.Equal("utf-16", builder.Encoding);
            Assert.Equal("Ver", builder.SqlMigrationPrefix);
            Assert.Equal("@", builder.SqlMigrationSeparator);
            Assert.Equal(".query", builder.SqlMigrationSuffix);
            Assert.Equal("my_shema", builder.Schemas.Single());
            Assert.Equal("my_metadata_schema", builder.MetadataTableSchema);
            Assert.Equal("metadata_store", builder.MetadataTableName);
            Assert.Equal("@{", builder.PlaceholderPrefix);
            Assert.Equal("@}", builder.PlaceholderSuffix);
            Assert.Equal("2_1_0", builder.TargetVersion);
            Assert.Equal("1_1_0", builder.StartVersion);
            Assert.True(new List<string>() { "Schema:my_schema", "Pwd:password" }.SequenceEqual(builder.Placeholders));
            Assert.Equal("true", builder.EraseDisabled);
            Assert.Equal("false", builder.EraseOnValidationError);
            Assert.Equal("migrate", builder.Command);
            Assert.Equal("false", builder.EnableClusterMode);
            Assert.Equal("false", builder.OutOfOrder);
            Assert.Null(builder.CommandTimeout);
        }

        [Fact]
        public void Build_CommandLine_Args_works()
        {
            string expected = @"postgresql Erase -c=""Server=127.0.0.1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;"" -l=""migration"" -l=""dataset"" -s=""my_shema"" --metadata-table-schema=""my_metadata_schema"" --metadata-table=""metadata_store"" -p=""Schema:my_schema"" -p=""Pwd:password"" --placeholder-prefix=@{ --placeholder-suffix=@} --target-version=2_1_0 --start-version=1_1_0 --scripts-prefix=Ver --scripts-suffix=.query --scripts-separator=@ --encoding=utf-16 --command-timeout=200 --out-of-order=true --erase-disabled=true --erase-on-validation-error=True --enable-cluster-mode=true";
            string actual = new JsonCliArgsBuilder(TestContext.EvolveJsonPath).Build();

            Assert.Equal(expected, actual);
        }
    }
}
