using Cassandra.Data;
using CommandLine;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Data;
using System.Data.SqlClient;

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

        static int Evolve(IDbConnection connection, Options options)
        {
            EvolveFactory.Build(connection, options).ExecuteCommand();

            return 0;
        }

        static int EvolveWithCassandra(CassandraOptions cassandraOptions) =>
            Evolve(new CqlConnection(cassandraOptions.ConnectionString), cassandraOptions);

        static int EvolveWithMySql(MySqlOptions mySqlOptions) =>
            Evolve(new MySqlConnection(mySqlOptions.ConnectionString), mySqlOptions);

        static int EvolveWithPostgreSql(PostgreSqlOptions postgreSqlOptions) =>
            Evolve(new NpgsqlConnection(postgreSqlOptions.ConnectionString), postgreSqlOptions);

        static int EvolveWithSQLite(SQLiteOptions sqLiteOptions) =>
            Evolve(new SqliteConnection(sqLiteOptions.ConnectionString), sqLiteOptions);

        static int EvolveWithSqlServer(SqlServerOptions sqlServerOptions) =>
            Evolve(new SqlConnection(sqlServerOptions.ConnectionString), sqlServerOptions);
    }
}
