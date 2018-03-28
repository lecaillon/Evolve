using Cassandra.Data;
using CommandLine;

namespace Evolve.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<CassandraOptions, MySqlOptions, PostgreSqlOptions, SQLiteOptions, SqlServerOptions>(args)
                .MapResult(
                    (CassandraOptions o) => EvolveWithCassandra(o),
                    (MySqlOptions o) => EvolveWithMySql(o),
                    (PostgreSqlOptions o) => EvolveWithPostgreSql(o),
                    (SqlServerOptions o) => EvolveWithSqlServer(o),
                    (SQLiteOptions o) => EvolveWithSQLite(o),
                    _ => 1);
        }


        static int EvolveWithCassandra(CassandraOptions cassandraOptions)
        {
            var evolve = EvolveFactory.Build(
                new CqlConnection(cassandraOptions.ConnectionString),
                cassandraOptions);

            evolve.ExecuteCommand();

            return 0;
        }

        static int EvolveWithMySql(MySqlOptions mySqlOptions)
        {
            return 0;
        }

        static int EvolveWithPostgreSql(PostgreSqlOptions postgreSqlOptions)
        {
            return 0;
        }

        static int EvolveWithSqlServer(SqlServerOptions sqlServerOptions)
        {
            return 0;
        }

        static int EvolveWithSQLite(SQLiteOptions sqLiteOptions)
        {
            return 0;
        }
    }
}
