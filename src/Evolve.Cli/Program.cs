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

            parser.ParseArguments<CassandraOptions, MySqlOptions, PostgreSqlOptions, SQLiteOptions, SqlServerOptions>(args)
                  .MapResult
                  (
                      (CassandraOptions options) => Evolve(options),
                      (MySqlOptions options) => Evolve(options),
                      (PostgreSqlOptions options) => Evolve(options),
                      (SqlServerOptions options) => Evolve(options),
                      (SQLiteOptions options) => Evolve(options),
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
