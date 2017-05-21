#if NETCORE

namespace Evolve.Driver
{
    /// <summary>
    ///     MySQL driver for projects targeting the .NET Standard/Core and build with a dotnet-build command.
    /// </summary>
    public class CoreMySqlDataDriver : CoreReflectionBasedDriver
    {
        public const string DriverAssemblyName = "MySql.Data";
        public const string ConnectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";

        public CoreMySqlDataDriver(string depsFile, string nugetPackageDir) 
            : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

namespace Evolve.Driver
{
    /// <summary>
    ///     MySQL driver for projects targeting the .NET Framework.
    /// </summary>
    public class MySqlDataDriver : NetReflectionBasedDriver
    {
        public const string DriverAssemblyName = "MySql.Data";
        public const string ConnectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";

        public MySqlDataDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }

#if NET45

    /// <summary>
    ///     MySQL driver for projects targeting the .NET Standard/Core and build with MSBuild.
    /// </summary>
    public class CoreMySqlDataDriverForNet : CoreReflectionBasedDriverForNet
    {
        public const string DriverAssemblyName = "MySql.Data";
        public const string ConnectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";

        public CoreMySqlDataDriverForNet(string depsFile, string nugetPackageDir) 
            : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }

#endif

}

#endif