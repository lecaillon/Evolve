using System;
using System.IO;
using CommandLine;

namespace Evolve.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(options =>
            {
                options.CaseSensitive = false;
                options.HelpWriter = Console.Error;
            });

            parser.ParseArguments<CassandraOptions, 
                                  MySqlOptions, MariaDbOptions, MySqlDataOptions, MySqlConnectorOptions, 
                                  PostgreSqlOptions, NpgsqlOptions,
                                  SQLiteOptions, SytemDataSQLiteOptions, MicrosoftSqliteOptions, MicrosoftDataSqliteOptions,
                                  SqlServerOptions, SqlClientOptions>(args)
                  .MapResult
                  (
                      (CassandraOptions options) => Evolve(options),

                      (MySqlOptions options) => Evolve(options),
                      (MariaDbOptions options) => Evolve(options),
                      (MySqlDataOptions options) => Evolve(options),
                      (MySqlConnectorOptions options) => Evolve(options),

                      (PostgreSqlOptions options) => Evolve(options),
                      (NpgsqlOptions options) => Evolve(options),


                      (SqlServerOptions options) => Evolve(options),
                      (SqlClientOptions options) => Evolve(options),

                      (SQLiteOptions options) => Evolve(options),
                      (SytemDataSQLiteOptions options) => Evolve(options),
                      (MicrosoftSqliteOptions options) => Evolve(options),
                      (MicrosoftDataSqliteOptions options) => Evolve(options),

                      _ => 1
                  );
        }

        static int Evolve(Options options)
        {
            string originalCurrentDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(options.TargetAppPath ?? originalCurrentDirectory);
                EvolveFactory.Build(options)
                             .ExecuteCommand();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
            finally
            {
                Directory.SetCurrentDirectory(originalCurrentDirectory);
            }
        }
    }
}
