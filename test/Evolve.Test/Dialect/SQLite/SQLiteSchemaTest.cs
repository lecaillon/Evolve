using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Evolve.Test.Dialect.SQLite
{
    public class SQLiteSchemaTest
    {
        [Fact(DisplayName = "blah")]
        public void blah()
        {
            string script = File.ReadAllText(@"C:\Users\Phil-Dev\Downloads\ChinookDatabase1.4_Sqlite\Chinook_Sqlite_AutoIncrementPKs.sql");

            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = script;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
