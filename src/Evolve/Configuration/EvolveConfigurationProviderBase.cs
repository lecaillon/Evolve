using Evolve.Utilities;

namespace Evolve.Configuration
{
    public abstract class EvolveConfigurationProviderBase : IConfigurationProvider
    {
        private string _configurationPath;
        private IEvolveConfiguration _configuration;

        public void Configure(string evolveConfigurationPath, IEvolveConfiguration configuration)
        {
            _configurationPath = Check.FileExists(evolveConfigurationPath, nameof(evolveConfigurationPath));
            _configuration = Check.NotNull(configuration, nameof(configuration));

            Configure();
            Validate();
        }

        protected virtual void Validate()
        {

        }

        protected abstract void Configure();
    }
}
