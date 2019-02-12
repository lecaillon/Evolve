using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Evolve.MSBuild;
using Xunit;

namespace Evolve.Tests.Configuration
{
    /// <summary>
    ///     Hack to be able to test this .NET only dedicated class in our .NET Core test project.
    ///     This class is a copy of the .NET class <see cref="MSBuild.AppConfigCliArgsBuilder"/>
    /// </summary>
    public class AppConfigCliArgsBuilder : CliArgsBuilder
    {
        private System.Configuration.Configuration _config = null;
        private Dictionary<string, string> _datasource = null;


        public AppConfigCliArgsBuilder(string configFile, string env = null)
            : base(configFile, env)
        {
            string cnn = _config.ConnectionStrings.ConnectionStrings[ConnectionString]?.ConnectionString;
            if (cnn != null)
            {
                ConnectionString = Normalize(cnn);
            }
        }

        protected override Dictionary<string, string> Datasource
        {
            get
            {
                if (_datasource == null)
                {
                    var configMap = new ExeConfigurationFileMap()
                    {
                        ExeConfigFilename = ConfigFile
                    };

                    try
                    {
                        _config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                    }
                    catch (Exception ex)
                    {
                        throw new EvolveMSBuildException(string.Format(IncorrectFileFormat, ConfigFile), ex);
                    }

                    var settings = _config.AppSettings.Settings;
                    _datasource = settings.AllKeys.ToDictionary(key => key, key => settings[key].Value, StringComparer.OrdinalIgnoreCase);
                }

                return _datasource;
            }
        }
    }

    public class AppConfigCliArgsBuilderTest
    {
        [Fact]
        [Category(Test.Configuration)]
        public void Load_app_configuration_file_works()
        {
            var builder = new AppConfigCliArgsBuilder(TestContext.EvolveAppConfigPath);

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
        [Category(Test.Configuration)]
        public void Load_incomplete_web_configuration_files_works()
        {
            var builder = new AppConfigCliArgsBuilder(TestContext.EvolveWebConfigPath);

            Assert.Equal("Erase", builder.Command);
            Assert.Equal("postgresql", builder.Database);
            Assert.Equal("Server=127.0.0.1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;", builder.ConnectionString);
            Assert.True(new List<string>() { "migration", "dataset" }.SequenceEqual(builder.Locations));
            Assert.Null(builder.Encoding);
            Assert.Null(builder.SqlMigrationPrefix);
            Assert.Null(builder.SqlMigrationSeparator);
            Assert.Null(builder.SqlMigrationSuffix);
            Assert.Null(builder.Schemas);
            Assert.Null(builder.MetadataTableSchema);
            Assert.Null(builder.MetadataTableName);
            Assert.Null(builder.PlaceholderPrefix);
            Assert.Null(builder.PlaceholderSuffix);
            Assert.Null(builder.TargetVersion);
            Assert.Null(builder.StartVersion);
            Assert.Empty(builder.Placeholders);
            Assert.Null(builder.EraseDisabled);
            Assert.Null(builder.EraseOnValidationError);
            Assert.Null(builder.EnableClusterMode);
            Assert.Null(builder.OutOfOrder);
            Assert.Null(builder.CommandTimeout);
        }

        [Fact]
        [Category(Test.Configuration)]
        public void Build_CommandLine_Args_works()
        {
            string expected = @"Erase postgresql -c=""Server=127.0.0.1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;"" -l=""migration"" -l=""dataset"" -s=""my_shema"" --metadata-table-schema=""my_metadata_schema"" --metadata-table=""metadata_store"" -p=""Schema:my_schema"" -p=""Pwd:password"" --placeholder-prefix=@{ --placeholder-suffix=@} --target-version=2_1_0 --start-version=1_1_0 --scripts-prefix=Ver --scripts-suffix=.query --scripts-separator=@ --encoding=utf-16 --command-timeout=200 --out-of-order=true --erase-disabled=true --erase-on-validation-error=True --enable-cluster-mode=true";
            string actual = new AppConfigCliArgsBuilder(TestContext.EvolveAppConfigPath).Build();

            Assert.Equal(expected, actual);
        }
    }
}
