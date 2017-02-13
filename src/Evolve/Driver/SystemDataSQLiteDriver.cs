namespace Evolve.Driver
{
    public class SystemDataSQLiteDriver : ReflectionBasedDriver
    {
        public const string DriverAssemblyName = "System.Data.SQLite";
        public const string ConnectionTypeName = "System.Data.SQLite.SQLiteConnection";

        public SystemDataSQLiteDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }
}
