using Evolve.Configuration;
using Evolve.Migration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve
{
    public class Evolve : IEvolveConfiguration, IMigrator
    {
        public Evolve(string evolveConfigurationPath)
        {
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
        public string MetadaTableSchema { get; set; }
        public string PlaceholderPrefix { get; set; }
        public string PlaceholderSuffix { get; set; }
        public string SqlMigrationPrefix { get; set; }
        public string SqlMigrationSeparator { get; set; }
        public string SqlMigrationSuffix { get; set; }
        public string TargetVersion { get; set; }

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
    }
}
