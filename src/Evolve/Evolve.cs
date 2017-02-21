using Evolve.Configuration;
using Evolve.Migration;
using Evolve.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve
{
    public class Evolve : IEvolveConfiguration, IMigrator
    {
        public Evolve()
        {
            // Set default configuration settings
            Encoding = Encoding.UTF8;
            Locations = new List<string> { "Sql_Scripts" };
            MetadaTableName = "changelog";
            PlaceholderPrefix = "${";
            PlaceholderSuffix = "}";
            SqlMigrationPrefix = "V";
            SqlMigrationSeparator = "__";
            SqlMigrationSuffix = ".sql";
        }

        #region IEvolveConfiguration

        public string ConnectionString { get; set; }
        public string DefaultSchema { get; set; }
        public string Driver { get; set; }
        public Encoding Encoding { get; set; }
        public IEnumerable<string> Locations { get; set; }
        public string MetadaTableName { get; set; }

        private string _metadaTableSchema;
        public string MetadaTableSchema
        {
            get { return string.IsNullOrWhiteSpace(_metadaTableSchema) ? DefaultSchema : _metadaTableSchema; }
            set { _metadaTableSchema = value; }
        }

        public string PlaceholderPrefix { get; set; }
        public string PlaceholderSuffix { get; set; }
        public string SqlMigrationPrefix { get; set; }
        public string SqlMigrationSeparator { get; set; }
        public string SqlMigrationSuffix { get; set; }
        public MigrationVersion TargetVersion { get; set; }

        #endregion

        #region IMigrator

        public string GenerateScript(string fromMigration = null, string toMigration = null)
        {
            throw new NotImplementedException();
        }

        public void Migrate(string targetVersion = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void Configure(string evolveConfigurationPath)
        {
            Check.FileExists(evolveConfigurationPath, nameof(evolveConfigurationPath));

            IConfigurationProvider configurationProvider = ConfigurationFactoryProvider.GetProvider(evolveConfigurationPath);
            configurationProvider.Configure(evolveConfigurationPath, this);
        }
    }
}
