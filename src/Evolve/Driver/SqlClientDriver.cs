#if NETCORE

namespace Evolve.Driver
{
    /// <summary>
    ///     SqlClient driver for projects targeting the .NET Standard/Core and build with a dotnet-build command.
    /// </summary>
    public class CoreSqlClientDriver : CoreReflectionBasedDriver
    {
        public const string DriverAssemblyName = "System.Data.SqlClient";
        public const string ConnectionTypeName = "System.Data.SqlClient.SqlConnection";

        public CoreSqlClientDriver(string depsFile, string nugetPackageDir) 
            : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

namespace Evolve.Driver
{
    using System.Data;
    using System.Data.SqlClient;

    /// <summary>
    ///     SqlClient driver for projects targeting the .NET Framework or .NET Standard/Core projects if build with MSBuild.
    /// </summary>
    public class SqlClientDriver : IDriver
    {
        /// <summary>
        ///     Creates an <see cref="IDbConnection"/> object for SQL Server.
        /// </summary>
        /// <param name="connectionString"> The connection string used to initialize the SqlConnection. </param>
        /// <returns> An initialized database connection. </returns>
        public IDbConnection CreateConnection(string connectionString)
        {
            var cnn = new SqlConnection();
            cnn.ConnectionString = connectionString;
            return cnn;
        }
    }
}

#endif
