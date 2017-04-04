#if NETCORE

namespace Evolve.Driver
{
    public class MySqlDataDriver : CoreReflectionBasedDriver
    {
        public const string DriverAssemblyName = "MySql.Data";
        public const string ConnectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";

        public MySqlDataDriver(string depsFile, string nugetPackageDir) : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

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

#endif