using System;
using System.IO;
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
                    (CassandraOptions o) => Evolve(o),
                    (MySqlOptions o) => Evolve(o),
                    (PostgreSqlOptions o) => Evolve(o),
                    (SqlServerOptions o) => Evolve(o),
                    (SQLiteOptions o) => Evolve(o),
                    _ => 1);
        }

        static int Evolve(Options options)
        {
            string originalCurrentDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(options.TargetAppPath);
                EvolveFactory.Build(options).ExecuteCommand();

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
