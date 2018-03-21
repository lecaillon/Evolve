#if NETCORE

namespace Evolve.Driver
{
    /// <summary>
    ///     MySQL driver for projects targeting the .NET Standard/Core and build with a dotnet-build command.
    /// </summary>
    public class CoreMySqlDataDriver : CoreReflectionBasedDriverEx
    {
        public const string DriverAssemblyName = "MySql.Data";
        public const string ConnectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";
        public const string NugetPackageName = "MySql.Data";

        public CoreMySqlDataDriver(string depsFile, string nugetPackageDir)
            : base(DriverAssemblyName, ConnectionTypeName, NugetPackageName, depsFile, nugetPackageDir)
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
    public class CoreMySqlDataDriverForNet : CoreReflectionBasedDriverForNetEx
    {
        public const string DriverAssemblyName = "MySql.Data";
        public const string ConnectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";
        public const string NugetPackageName = "MySql.Data";

        public CoreMySqlDataDriverForNet(string depsFile, string nugetPackageDir, string msBuildExtensionsPath) 
            : base(DriverAssemblyName, ConnectionTypeName, NugetPackageName, depsFile, nugetPackageDir, msBuildExtensionsPath)
        {
        }
    }

#endif

}

#endif