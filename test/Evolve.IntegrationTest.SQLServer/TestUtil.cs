using System.Data.SqlClient;

namespace Evolve.IntegrationTest.SQLServer
{
    public static class TestUtil
    {
        public static void CreateTestDatabase(string dbName, string user, string password)
        {
            var cnn = new SqlConnection($"Server=127.0.0.1;Database=master;User Id={user};Password={password};");
            cnn.Open();

            using (var cmd = cnn.CreateCommand())
            {
                cmd.CommandText = $"CREATE DATABASE {dbName};";
                cmd.ExecuteNonQuery();
            }

            cnn.Close();
        }
    }
}
