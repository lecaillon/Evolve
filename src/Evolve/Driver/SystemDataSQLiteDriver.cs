#if NET

namespace Evolve.Driver
{
    /// <summary>
    ///     System.Data.SQlite driver for projects targeting the .NET Framework.
    /// </summary>
    /// <remarks>
    ///     This driver does not support .NET Core.
    /// </remarks>
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