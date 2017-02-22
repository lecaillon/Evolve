using Evolve.Configuration;
using Xunit;

namespace Evolve.Test.Configuration
{
    public class AppConfigConfigurationProviderTest
    {
        [Fact()]
        public void LoadPSG()
        {
            var evolve = new Evolve();
            var configurationProvider = new AppConfigConfigurationProvider();
            configurationProvider.Configure(TestContext.AppConfigPath, evolve);

        }
    }
}
