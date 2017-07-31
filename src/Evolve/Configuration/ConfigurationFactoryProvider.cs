using System;
using System.Collections.Generic;
using System.IO;
using Evolve.Utilities;

namespace Evolve.Configuration
{
    public static class ConfigurationFactoryProvider
    {
        private const string NotSupportedConfigurationFile = "Evolve only supports App.config, Web.config or Evolve.json files.";
        private static readonly Dictionary<string, Func<IConfigurationProvider>> _providers = new Dictionary<string, Func<IConfigurationProvider>>
        {
#if NET
            [".config"] = () => new AppConfigConfigurationProvider(),
#endif
#if NETCORE || NET45
            [".json"] = () => new JsonConfigurationProvider(),
#endif
        };

        public static IConfigurationProvider GetProvider(string evolveConfigurationPath)
        {
            Check.FileExists(evolveConfigurationPath, nameof(evolveConfigurationPath));

            string ext = Path.GetExtension(evolveConfigurationPath);
            _providers.TryGetValue(ext, out Func<IConfigurationProvider> providerCreationDelegate);
            if (providerCreationDelegate == null)
            {
                throw new EvolveConfigurationException(NotSupportedConfigurationFile);
            }

            return providerCreationDelegate();
        }
    }
}
