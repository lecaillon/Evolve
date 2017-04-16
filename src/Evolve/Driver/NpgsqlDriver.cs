#if NETCORE

namespace Evolve.Driver
{
    /// <summary>
    ///     Npgsql (PostgreSQL) driver for projects targeting the .NET Standard/Core and build with a dotnet-build command.
    /// </summary>
    public class CoreNpgsqlDriver : CoreReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Npgsql";
        public const string ConnectionTypeName = "Npgsql.NpgsqlConnection";

        public CoreNpgsqlDriver(string depsFile, string nugetPackageDir) 
            : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

namespace Evolve.Driver
{
    /// <summary>
    ///     Npgsql (PostgreSQL) driver for projects targeting the .NET Framework.
    /// </summary>
    public class NpgsqlDriver : NetReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Npgsql";
        public const string ConnectionTypeName = "Npgsql.NpgsqlConnection";

        public NpgsqlDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }

#if NET45

    /// <summary>
    ///     Npgsql (PostgreSQL) driver for projects targeting the .NET Standard/Core and build with MSBuild.
    /// </summary>
    public class CoreNpgsqlDriverForNet : CoreReflectionBasedDriverForNet
    {
        public const string DriverAssemblyName = "Npgsql";
        public const string ConnectionTypeName = "Npgsql.NpgsqlConnection";

        public CoreNpgsqlDriverForNet(string depsFile, string nugetPackageDir) 
            : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }

#endif

}

#endif