using CommandLine;

namespace Evolve.Cli
{
    [Verb("sqlite", HelpText = "Evolve with SQLite (driver: System.Data.SQLite, .NET only)")]
    internal class SQLiteOptions : SqlOptions
    {
        public override string Driver => "sqlite";
    }

    [Verb("System.Data.SQLite", Hidden = true, HelpText = "Evolve with SQLite (driver: System.Data.SQLite, .NET only)")]
    internal class SytemDataSQLiteOptions : SqlOptions
    {
        public override string Driver => "systemdatasqlite";
    }

    [Verb("microsoftsqlite", HelpText = "Evolve with SQLite (driver: Microsoft.Data.Sqlite)")]
    internal class MicrosoftSqliteOptions : SqlOptions
    {
        public override string Driver => "microsoftsqlite";
    }

    [Verb("Microsoft.Data.Sqlite", Hidden = true, HelpText = "Evolve with SQLite (driver: Microsoft.Data.Sqlite)")]
    internal class MicrosoftDataSqliteOptions : SqlOptions
    {
        public override string Driver => "microsoftdatasqlite";
    }
}
