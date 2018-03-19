#if NETCORE

namespace Evolve.Driver
{
    /// <summary>
    ///     Cassandra driver for project targeting the .NET Standard/Core and build with dotnet-build command.
    /// </summary>
    public class CoreCassandraDriver : CoreReflectionBasedDriverEx
    {
        public const string DriverAssemblyName = "Cassandra";
        public const string ConnectionTypeName = "Cassandra.Data.CqlConnection";

        public CoreCassandraDriver(string depsFile, string nugetPackageDir)
            : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

namespace Evolve.Driver
{
    /// <summary>
    ///     Cassandra driver for projects targeting the .NET Framework.
    /// </summary>
    public class CassandraDriver : NetReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Cassandra";
        public const string ConnectionTypeName = "Cassandra.Data.CqlConnection";

        public CassandraDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }

#if NET45

    /// <summary>
    ///     Cassandra driver for projects targeting the .NET Standard/Core and build with MSBuild.
    /// </summary>
    public class CoreCassandraDriverForNet : CoreReflectionBasedDriverForNetEx
    {
        public const string DriverAssemblyName = "Cassandra";
        public const string ConnectionTypeName = "Cassandra.Data.CqlConnection";

        public CoreCassandraDriverForNet(string depsFile, string nugetPackageDir, string msBuildExtensionsPath) 
            : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir, msBuildExtensionsPath)
        {
        }
    }

#endif

}

#endif