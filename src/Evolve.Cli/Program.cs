using Cassandra.Data;
using CommandLine;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Data;
using System.Data.SqlClient;

#if NETCORE
using Microsoft.Data.Sqlite;
#elif NET
using System.Data.SQLite;
#endif

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
#if NETCORE
            Evolve(new SqliteConnection(sqLiteOptions.ConnectionString), sqLiteOptions);
#elif NET
            Evolve(new SQLiteConnection(sqLiteOptions.ConnectionString), sqLiteOptions);
#endif

        static int EvolveWithSqlServer(SqlServerOptions sqlServerOptions) =>
            Evolve(new SqlConnection(sqlServerOptions.ConnectionString), sqlServerOptions);
    }
}
