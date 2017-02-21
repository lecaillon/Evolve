using Evolve.Driver;
using Xunit;

namespace Evolve.Test.Core.Driver
{
    public class MicrosoftDataSqliteDriverTest
    {
        [Fact(DisplayName = "Load_ConnectionType_from_an_already_loaded_assembly")]
        public void Load_ConnectionType_from_an_already_loaded_assembly()
        {
            var driver = new MicrosoftDataSqliteDriver();
            Assert.NotNull(driver.DbConnectionType);
        }
    }
}
