#if NETCORE

namespace Evolve.Driver
{
    /// <summary>
    ///     Microsoft.Data.SQlite driver for projects targeting the .NET Standard/Core and build with a dotnet-build command.
    /// </summary>
    public class CoreMicrosoftDataSqliteDriver : CoreReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Microsoft.Data.Sqlite";
        public const string ConnectionTypeName = "Microsoft.Data.Sqlite.SqliteConnection";

        public CoreMicrosoftDataSqliteDriver(string depsFile, string nugetPackageDir) 
            : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

namespace Evolve.Driver
{
    /// <summary>
    ///     Microsoft.Data.SQlite driver for projects targeting the .NET Framework.
    /// </summary>
    public class MicrosoftDataSqliteDriver : NetReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Microsoft.Data.Sqlite";
        public const string ConnectionTypeName = "Microsoft.Data.Sqlite.SqliteConnection";

        public MicrosoftDataSqliteDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }

#if NET45

    /// <summary>
    ///     Microsoft.Data.SQlite driver for projects targeting the .NET Standard/Core and build with MSBuild.
    /// </summary>
    public class CoreMicrosoftDataSqliteDriverForNet : CoreReflectionBasedDriverForNet
    {
        public const string DriverAssemblyName = "Microsoft.Data.Sqlite";
        public const string ConnectionTypeName = "Microsoft.Data.Sqlite.SqliteConnection";

        public CoreMicrosoftDataSqliteDriverForNet(string depsFile, string nugetPackageDir) 
            : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }

#endif
}

#endif