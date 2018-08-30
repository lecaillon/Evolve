using CommandLine;

namespace Evolve.Cli
{
    [Verb("mysql", HelpText = "Evolve with MySQL (driver: MySql.Data)")]
    internal class MySqlOptions : SqlOptions
    {
        public override string Driver => "mysql";
    }

    [Verb("mariadb", HelpText = "Evolve with MariaDB (driver: MySql.Data)")]
    internal class MariaDbOptions : SqlOptions
    {
        public override string Driver => "mariadb";
    }

    [Verb("MySql.Data", Hidden = true, HelpText = "Evolve with MySQL / MariaDB (driver: MySql.Data)")]
    internal class MySqlDataOptions : SqlOptions
    {
        public override string Driver => "mysqldata";
    }

    [Verb("mysqlconnector", HelpText = "Evolve with MySQL / MariaDB (driver: MySqlConnector)")]
    internal class MySqlConnectorOptions : SqlOptions
    {
        public override string Driver => "mysqlconnector";
    }
}
