using System;
using System.Collections.Generic;

namespace Evolve.Configuration
{
    public static class ConfigurationFactoryProvider
    {
        static readonly Dictionary<string, IConfigurationProvider> providers = new Dictionary<string, IConfigurationProvider>
        {
#if NET452
            [".config"] = new AppConfigConfigurationProvider(),
#endif
            [".json"] = new JsonConfigurationProvider(),
        };

        public static IConfigurationProvider GetProvider(string evolveConfigurationPath)
        {
            throw new NotImplementedException();
        }
    }
}
