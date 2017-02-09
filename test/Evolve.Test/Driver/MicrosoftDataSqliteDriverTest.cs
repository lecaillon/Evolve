using Evolve.Driver;
using Xunit;

namespace Evolve.Test.Driver
{
    public class MicrosoftDataSqliteDriverTest
    {
        [Fact(DisplayName = "Load_ConnectionType_from_an_already_loaded_assembly")]
        public void Load_ConnectionType_from_an_already_loaded_assembly()
        {
            var driver = new MicrosoftDataSqliteDriver();
            Assert.NotNull(driver.ConnectionType);
        }
    }
}

// http://stackoverflow.com/questions/37895278/how-to-load-assemblies-located-in-a-folder-in-net-core-console-app
// http://www.michael-whelan.net/replacing-appdomain-in-dotnet-core/
// https://github.com/aspnet/Announcements/issues/149
