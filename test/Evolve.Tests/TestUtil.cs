using System.Data.SqlClient;
using System.Data.SQLite;
using Evolve.Connection;

namespace Evolve.Tests
{
    public static class TestUtil
    {
        public static void CreateSqlServerDatabase(string dbName, string cnxStr)
        {
            var cnn = new SqlConnection(cnxStr);
            cnn.Open();

            using (var cmd = cnn.CreateCommand())
            {
                cmd.CommandText = $"CREATE DATABASE {dbName};";
                cmd.ExecuteNonQuery();
            }

            cnn.Close();
        }

        public static WrappedConnection CreateSQLiteWrappedCnx() => new WrappedConnection(new SQLiteConnection("Data Source=:memory:"));
    }
}
