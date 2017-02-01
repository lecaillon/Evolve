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
            string script = File.ReadAllText(TestContext.ChinookScriptPath);

            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = script;
                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT tbl_name FROM \"main\".sqlite_master WHERE type = 'table'";
                    List<string> tables = new List<string>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }

                }
            }
        }
    }
}
