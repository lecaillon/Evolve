using System;
using System.Collections.Generic;
using System.IO;
using Evolve.Utilities;

namespace Evolve.Configuration
{
    public static class ConfigurationFactoryProvider
    {
        private const string NotSupportedConfigurationFile = "Evolve only supports App.config, Web.config or .json files.";
        private static readonly Dictionary<string, Func<IConfigurationProvider>> _providers = new Dictionary<string, Func<IConfigurationProvider>>
        {
#if NET
            [".config"] = () => new AppConfigConfigurationProvider(),
#endif
            [".json"] = () => new JsonConfigurationProvider(),
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
