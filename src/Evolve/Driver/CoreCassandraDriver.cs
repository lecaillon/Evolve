#if NETCORE

namespace Evolve.Driver
{
    /// <summary>
    ///     Cassandra driver for project targeting the .NET Standard/Core and build with dotnet-build command.
    /// </summary>
    public class CoreCassandraDriver : CoreReflectionBasedDriverEx
    {
        private const string DriverAssemblyName = "Cassandra";
        private const string ConnectionTypeName = "Cassandra.Data.CqlConnection";
        private const string NugetPackageId = "CassandraCSharpDriver";

        public CoreCassandraDriver(string depsFile, string nugetPackageDir)
            : base(DriverAssemblyName, ConnectionTypeName, NugetPackageId, depsFile, nugetPackageDir)
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
        private const string DriverAssemblyName = "Cassandra";
        private const string ConnectionTypeName = "Cassandra.Data.CqlConnection";
        private const string NugetPackageId = "CassandraCSharpDriver";

        public CoreCassandraDriverForNet(string depsFile, string nugetPackageDir, string msBuildExtensionsPath) 
            : base(DriverAssemblyName, ConnectionTypeName, NugetPackageId, depsFile, nugetPackageDir, msBuildExtensionsPath)
        {
        }
    }

#endif

}

#endif