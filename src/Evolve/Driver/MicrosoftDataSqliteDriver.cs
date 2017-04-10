#if NETSTANDARD

namespace Evolve.Driver
{
    public class MicrosoftDataSqliteDriver : CoreReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Microsoft.Data.Sqlite";
        public const string ConnectionTypeName = "Microsoft.Data.Sqlite.SqliteConnection";

        public MicrosoftDataSqliteDriver(string depsFile, string nugetPackageDir) : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

namespace Evolve.Driver
{
    public class MicrosoftDataSqliteDriver : NetReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Microsoft.Data.Sqlite";
        public const string ConnectionTypeName = "Microsoft.Data.Sqlite.SqliteConnection";

        public MicrosoftDataSqliteDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }
}

#endif