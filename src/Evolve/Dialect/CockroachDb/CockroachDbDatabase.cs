using System.Linq;
using Evolve.Connection;

namespace Evolve.Dialect.CockroachDB
{
    internal class CockroachDBDatabase : Schema
    {
        public CockroachDBDatabase(string schemaName, WrappedConnection wrappedConnection)
            : base(schemaName, wrappedConnection)
        {
        }

        public override bool IsExists() => _wrappedConnection.QueryForLong($"SELECT COUNT(*) FROM pg_database WHERE datname = '{Name}'") > 0;

        public override bool IsEmpty()
        {
            string sql = "SELECT " +
                            $"(SELECT COUNT(*) " +
                            $"FROM \"{Name}\".information_schema.tables " +
                            $"WHERE table_catalog = '{Name}' AND table_schema = 'public' AND table_type = 'BASE TABLE') " +
                             " + " +
                            $"(SELECT COUNT(*) " +
                            $"FROM \"{Name}\".information_schema.sequences " +
                            $"WHERE sequence_catalog = '{Name}' AND sequence_schema = 'public')";

            return _wrappedConnection.QueryForLong(sql) == 0;
        }

        public override bool Create()
        {
            _wrappedConnection.ExecuteNonQuery($"CREATE DATABASE \"{Name}\"");
            return true;
        }

        public override bool Drop()
        {
            _wrappedConnection.ExecuteNonQuery($"DROP DATABASE IF EXISTS \"{Name}\"");
            return true;
        }

        public override bool Erase()
        {
            DropViews();
            DropTables();
            DropSequences();

            return true;
        }

        protected void DropViews()
        {
            string sql = "SELECT table_name " +
                        $"FROM \"{Name}\".information_schema.views " +
                        $"WHERE table_catalog = '{Name}' " +
                         "AND table_schema = 'public'";

            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(view =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP VIEW IF EXISTS \"{Name}\".\"{Quote(view)}\" CASCADE");
            });
        }

        protected void DropTables()
        {
            string sql = "SELECT table_name " +
                        $"FROM \"{Name}\".information_schema.tables " +
                        $"WHERE table_catalog = '{Name}' " +
                         "AND table_schema = 'public' " +
                         "AND table_type = 'BASE TABLE'";

            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(table =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP TABLE IF EXISTS \"{Name}\".\"{Quote(table)}\" CASCADE");
            });
        }

        protected void DropSequences()
        {
            string sql = "SELECT sequence_name " +
                        $"FROM \"{Name}\".information_schema.sequences " +
                        $"WHERE sequence_catalog = '{Name}' " +
                         "AND sequence_schema = 'public'";

            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(seq =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP SEQUENCE IF EXISTS \"{Name}\".\"{Quote(seq)}\"");
            });
        }

        private static string Quote(string dbObject) => dbObject.Replace("\"", "\"\"");
    }
}
