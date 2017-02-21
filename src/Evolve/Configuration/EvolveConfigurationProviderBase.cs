using Evolve.Utilities;

namespace Evolve.Configuration
{
    public abstract class EvolveConfigurationProviderBase : IConfigurationProvider
    {
        protected string _configurationPath;
        protected IEvolveConfiguration _configuration;

        protected const string Key_SqlMigrationPrefix = "Evolve.SqlMigrationPrefix";

        public void Configure(string evolveConfigurationPath, IEvolveConfiguration configuration)
        {
            _configurationPath = Check.FileExists(evolveConfigurationPath, nameof(evolveConfigurationPath));
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
