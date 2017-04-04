using System.Data;
using Evolve.Driver;
using Xunit;

namespace Evolve.Test.Driver
{
    public class SystemDataSQLiteDriverTest
    {
        [Fact(DisplayName = "Load_ConnectionType_from_an_already_loaded_assembly")]
        public void Load_ConnectionType_from_an_already_loaded_assembly()
        {
            var driver = new SystemDataSQLiteDriver();
            var cnn = driver.CreateConnection("Data Source=:memory:");
            cnn.Open();

            Assert.True(cnn.State == ConnectionState.Open);
        }
    }
}
