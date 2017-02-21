using System;
using Evolve.Utilities;

namespace Evolve.Configuration
{
    public abstract class EvolveConfigurationProviderBase : IConfigurationProvider
    {
        protected string _filePath;
        protected IEvolveConfiguration _configuration;

        protected const string ConnectionString = "Evolve.ConnectionString";
        protected const string Driver = "Evolve.Driver";
        protected const string Locations = "Evolve.Locations";
        protected const string Encoding = "Evolve.Encoding";
        protected const string SqlMigrationPrefix = "Evolve.SqlMigrationPrefix";
        protected const string SqlMigrationSeparator = "Evolve.SqlMigrationSeparator";
        protected const string SqlMigrationSuffix = "Evolve.SqlMigrationSuffix";
        protected const string DefaultSchema = "Evolve.DefaultSchema";
        protected const string MetadaTableSchema = "Evolve.MetadaTableSchema";
        protected const string MetadaTableName = "Evolve.MetadaTableName";
        protected const string PlaceholderPrefix = "Evolve.PlaceholderPrefix";
        protected const string PlaceholderSuffix = "Evolve.PlaceholderSuffix";
        protected const string TargetVersion = "Evolve.TargetVersion";

        public void Configure(string evolveConfigurationPath, IEvolveConfiguration configuration)
        {
            _filePath = Check.FileExists(evolveConfigurationPath, nameof(evolveConfigurationPath));
            _configuration = Check.NotNull(configuration, nameof(configuration));

            Configure();
            Validate();
        }

        protected abstract void Configure();

        protected virtual void Validate()
        {
        }
    }
}
