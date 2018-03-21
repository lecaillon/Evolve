#if NETCORE

namespace Evolve.Driver
{
    /// <summary>
    ///     Microsoft.Data.SQlite driver for projects targeting the .NET Standard/Core and build with a dotnet-build command.
    /// </summary>
    public class CoreMicrosoftDataSqliteDriver : CoreReflectionBasedDriverEx
    {
        public const string DriverAssemblyName = "Microsoft.Data.Sqlite";
        public const string ConnectionTypeName = "Microsoft.Data.Sqlite.SqliteConnection";
        public const string NugetPackageName = "Microsoft.Data.Sqlite";

        public CoreMicrosoftDataSqliteDriver(string depsFile, string nugetPackageDir)
            : base(DriverAssemblyName, ConnectionTypeName, NugetPackageName, depsFile, nugetPackageDir)
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
    public class CoreMicrosoftDataSqliteDriverForNet : CoreReflectionBasedDriverForNetEx
    {
        public const string DriverAssemblyName = "Microsoft.Data.Sqlite";
        public const string ConnectionTypeName = "Microsoft.Data.Sqlite.SqliteConnection";
        public const string NugetPackageName = "Microsoft.Data.Sqlite";

        public CoreMicrosoftDataSqliteDriverForNet(string depsFile, string nugetPackageDir, string msBuildExtensionsPath)
            : base(DriverAssemblyName, ConnectionTypeName, NugetPackageName, depsFile, nugetPackageDir, msBuildExtensionsPath)
        {
        }
    }

#endif
}

#endif