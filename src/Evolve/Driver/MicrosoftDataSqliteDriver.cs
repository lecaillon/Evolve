namespace Evolve.Driver
{
    public class MicrosoftDataSqliteDriver : ReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Microsoft.Data.Sqlite";
        public const string ConnectionTypeName = "Microsoft.Data.Sqlite.SqliteConnection";

        public MicrosoftDataSqliteDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }
}
