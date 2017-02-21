using Evolve.Configuration;
using Xunit;

namespace Evolve.Test.Configuration
{
    public class AppConfigConfigurationProviderTest
    {
        [Fact()]
        public void Test()
        {
            var configurationProvider = new AppConfigConfigurationProvider();
            configurationProvider.Configure("", new Evolve());

        }
    }
}
