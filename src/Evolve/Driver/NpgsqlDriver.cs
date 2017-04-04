#if NETCORE

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
    public class NpgsqlDriver : ReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Npgsql";
        public const string ConnectionTypeName = "Npgsql.NpgsqlConnection";

        public NpgsqlDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }
}

#endif