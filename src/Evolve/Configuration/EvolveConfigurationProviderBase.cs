using System.Linq;
using Evolve.Utilities;

namespace Evolve.Configuration
{
    public abstract class EvolveConfigurationProviderBase : IConfigurationProvider
    {
        private const string ValueCannotBeNull = "Configuration parameter [{0}] cannot be null or empty. Update your Evolve configuration file at: {1}.";
        protected string _filePath;
        protected IEvolveConfiguration _configuration;

        protected const string ConnectionString = "Evolve.ConnectionString";
        protected const string Driver = "Evolve.Driver";
        protected const string Locations = "Evolve.Locations";
        protected const string Encoding = "Evolve.Encoding";
        protected const string SqlMigrationPrefix = "Evolve.SqlMigrationPrefix";
        protected const string SqlMigrationSeparator = "Evolve.SqlMigrationSeparator";
        protected const string SqlMigrationSuffix = "Evolve.SqlMigrationSuffix";
        protected const string Schemas = "Evolve.Schemas";
        protected const string MetadaTableSchema = "Evolve.MetadaTableSchema";
        protected const string MetadaTableName = "Evolve.MetadaTableName";
        protected const string PlaceholderPrefix = "Evolve.PlaceholderPrefix";
        protected const string PlaceholderSuffix = "Evolve.PlaceholderSuffix";
        protected const string TargetVersion = "Evolve.TargetVersion";
        protected const string Placeholder = "Evolve.Placeholder.";

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
            IfNullOrWhiteSpaceThrowsException(_configuration.SqlMigrationPrefix, SqlMigrationPrefix);
            IfNullOrWhiteSpaceThrowsException(_configuration.SqlMigrationSeparator, SqlMigrationSeparator);
            IfNullOrWhiteSpaceThrowsException(_configuration.SqlMigrationSuffix, SqlMigrationSuffix);
            IfNullOrWhiteSpaceThrowsException(_configuration.MetadaTableName, MetadaTableName);
            IfNullOrWhiteSpaceThrowsException(_configuration.PlaceholderPrefix, PlaceholderPrefix);
            IfNullOrWhiteSpaceThrowsException(_configuration.PlaceholderSuffix, PlaceholderSuffix);

            if (_configuration.Locations == null || _configuration.Locations.Count() == 0)
            {
                throw new EvolveConfigurationException(string.Format(ValueCannotBeNull, Locations, _filePath));
            }

            if (_configuration.Encoding == null)
            {
                throw new EvolveConfigurationException(string.Format(ValueCannotBeNull, Encoding, _filePath));
            }
        }

        private void IfNullOrWhiteSpaceThrowsException(string value, string name)
        {
            if (value.IsNullOrWhiteSpace())
            {
                throw new EvolveConfigurationException(string.Format(ValueCannotBeNull, name, _filePath));
            }
        }
    }
}
