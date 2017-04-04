#if NETCORE

namespace Evolve.Driver
{
    public class SqlClientDriver : CoreReflectionBasedDriver
    {
        public const string DriverAssemblyName = "System.Data.SqlClient";
        public const string ConnectionTypeName = "System.Data.SqlClient.SqlConnection";

        public SqlClientDriver(string depsFile, string nugetPackageDir) : base(DriverAssemblyName, ConnectionTypeName, depsFile, nugetPackageDir)
        {
        }
    }
}

#else

namespace Evolve.Driver
{
    /// <summary>
    ///     <para>
    ///         Creates an IDbConnection object for the specific Driver.
    ///     </para>
    ///     <para>
    ///         The connectionString is used to open a connection to the database to
    ///         force a load of the driver while the application current directory
    ///         is temporary changed to a folder where are stored the native dependencies.
    ///     </para>
    /// </summary>
    /// <param name="connectionString"> The connection string. </param>
    /// <returns> An IDbConnection object for the specific Driver. </returns>
    public class SqlClientDriver : IDriver
    {
        public System.Data.IDbConnection CreateConnection(string connectionString)
        {
            var cnn = new System.Data.SqlClient.SqlConnection();
            cnn.ConnectionString = connectionString;
            return cnn;
        }
    }
}

#endif
