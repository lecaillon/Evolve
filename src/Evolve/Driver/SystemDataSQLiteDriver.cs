#if NETSTANDARD

namespace Evolve.Driver
{
    public class SystemDataSQLiteDriver : CoreReflectionBasedDriver
    {
        public const string DriverAssemblyName = "System.Data.SQLite";
        public const string ConnectionTypeName = "System.Data.SQLite.SQLiteConnection";

        public SystemDataSQLiteDriver(string depsFile, string nugetPackageDir) : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

namespace Evolve.Driver
{
    public class SystemDataSQLiteDriver : NetReflectionBasedDriver
    {
        public const string DriverAssemblyName = "System.Data.SQLite";
        public const string ConnectionTypeName = "System.Data.SQLite.SQLiteConnection";

        public SystemDataSQLiteDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }
}

#endif