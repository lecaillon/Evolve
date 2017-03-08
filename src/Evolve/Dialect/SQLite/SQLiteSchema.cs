using System.Collections.Generic;
using System.Linq;
using Evolve.Connection;

namespace Evolve.Dialect.SQLite
{
    public class SQLiteSchema : Schema
    {
        private static List<string> IgnoredSystemTableNames = new List<string> { "android_metadata", "sqlite_sequence" };
        private static List<string> UndroppableTableNames = new List<string> { "sqlite_sequence" };

        public SQLiteSchema(WrappedConnection wrappedConnection) : base("main", wrappedConnection)
        {
        }

        public override bool IsExists()
        {
            try
            {
                GetTables();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool IsEmpty()
        {
            return GetTables().Except(IgnoredSystemTableNames).Count() == 0;
        }

        /// <summary>
        ///     SQLite does not support creating schemas.
        /// </summary>
        /// <returns> false. </returns>
        public override bool Create() => false;

        /// <summary>
        ///     SQLite does not support dropping schemas.
        /// </summary>
        /// <returns> false. </returns>
        public override bool Drop() => false;

        public override bool Erase()
        {
            DropViews();
            DropTables();
            DropSequences();

            return true;
        }

        protected List<string> GetTables()
        {
            return _wrappedConnection.QueryForListOfString($"SELECT tbl_name FROM sqlite_master WHERE type = 'table'").ToList();
        }

        protected void DropTables()
        {
            GetTables().Except(UndroppableTableNames).ToList().ForEach(t =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP TABLE {t}");
            });
        }

        protected void DropViews()
        {
            string sql = $"SELECT tbl_name FROM sqlite_master WHERE type = 'view'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(vw =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP VIEW {vw}");
            });
        }

        protected void DropSequences()
        {
            string sql = $"SELECT COUNT(tbl_name) FROM sqlite_master WHERE type = 'table' AND tbl_name = 'sqlite_sequence'";
            if(_wrappedConnection.QueryForLong(sql) == 1)
            {
                _wrappedConnection.ExecuteNonQuery($"DELETE FROM sqlite_sequence");
            }
        }
    }
}
