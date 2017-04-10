#if NETSTANDARD

namespace Evolve.Driver
{
    public class NpgsqlDriver : CoreReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Npgsql";
        public const string ConnectionTypeName = "Npgsql.NpgsqlConnection";

        public NpgsqlDriver(string depsFile, string nugetPackageDir) : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

namespace Evolve.Driver
{
    public class NpgsqlDriver : NetReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Npgsql";
        public const string ConnectionTypeName = "Npgsql.NpgsqlConnection";

        public NpgsqlDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }

#if NET45

    public class CoreNpgsqlDriverForNet : CoreReflectionBasedDriverForNet
    {
        public const string DriverAssemblyName = "Npgsql";
        public const string ConnectionTypeName = "Npgsql.NpgsqlConnection";

        public CoreNpgsqlDriverForNet(string depsFile, string nugetPackageDir) : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }

#endif

}

#endif