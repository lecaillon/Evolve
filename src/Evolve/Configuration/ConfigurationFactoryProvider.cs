using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Configuration
{
    public static class ConfigurationFactoryProvider
    {
        static Dictionary<string, IConfigurationProvider> providers = new Dictionary<string, IConfigurationProvider>
        {
            [".config"] = new AppConfigConfigurationProvider(),
            [".json"] = new JsonConfigurationProvider(),
        };

        public static IConfigurationProvider GetProvider(string evolveConfigurationPath)
        {
            throw new NotImplementedException();
        }
    }
}
