namespace Evolve.Driver
{
    public class MySqlDataDriver : ReflectionBasedDriver
    {
        public const string DriverAssemblyName = "MySql.Data";
        public const string ConnectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";

        public MySqlDataDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }
}
