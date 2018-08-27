#if NETCORE

namespace Evolve.Driver
{
    /// <summary>
    ///     MySqlConnector driver for projects targeting the .NET Standard/Core and build with a dotnet-build command.
    /// </summary>
    public class CoreMySqlConnectorDriver : CoreReflectionBasedDriverEx
    {
        public const string DriverAssemblyName = "MySqlConnector";
        public const string ConnectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";
        public const string NugetPackageId = "MySqlConnector";

        public CoreMySqlConnectorDriver(string depsFile, string nugetPackageDir)
            : base(DriverAssemblyName, ConnectionTypeName, NugetPackageId, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

namespace Evolve.Driver
{
    /// <summary>
    ///     MySqlConnector driver for projects targeting the .NET Framework.
    /// </summary>
    public class MySqlConnectorDriver : NetReflectionBasedDriver
    {
        public const string DriverAssemblyName = "MySqlConnector";
        public const string ConnectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";

        public MySqlConnectorDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }

#if NET45

    /// <summary>
    ///     MySqlConnector driver for projects targeting the .NET Standard/Core and build with MSBuild.
    /// </summary>
    public class CoreMySqlConnectorDriverForNet : CoreReflectionBasedDriverForNetEx
    {
        public const string DriverAssemblyName = "MySqlConnector";
        public const string ConnectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";
        public const string NugetPackageId = "MySqlConnector";

        public CoreMySqlConnectorDriverForNet(string depsFile, string nugetPackageDir, string msBuildExtensionsPath) 
            : base(DriverAssemblyName, ConnectionTypeName, NugetPackageId, depsFile, nugetPackageDir, msBuildExtensionsPath)
        {
        }
    }

#endif

}

#endif