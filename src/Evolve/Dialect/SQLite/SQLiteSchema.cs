using System.Collections.Generic;
using System.Linq;
using Evolve.Connection;

namespace Evolve.Dialect.SQLite
{
    public class SQLiteSchema : Schema
    {
        private static List<string> IgnoredSystemTableNames = new List<string> { "android_metadata", "sqlite_sequence" };
        private static List<string> UndroppableTableNames = new List<string> { "sqlite_sequence" };

        public SQLiteSchema(string schemaName, IWrappedConnection wrappedConnection) : base(schemaName, wrappedConnection)
        {
        }

        public override bool IsExists()
        {
            try
            {
                GetAllTables();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool IsEmpty()
        {
            return GetAllTables().Except(IgnoredSystemTableNames).Count() == 0;
        }

        /// <summary>
        ///     SQLite does not support creating schemas.
        /// </summary>
        /// <returns> false. </returns>
        public override bool Create()
        {
            return false;
        }

        /// <summary>
        ///     SQLite does not support dropping schemas.
        /// </summary>
        /// <returns> false. </returns>
        public override bool Drop()
        {
            return false;
        }

        public override bool Clean()
        {
            CleanViews();
            CleanTables();
            CleanSequences();

            return true;
        }

        protected List<string> GetAllTables()
        {
            string sql = $"SELECT tbl_name FROM \"{Name}\".sqlite_master WHERE type = 'table'";
            return _wrappedConnection.DbConnection.QueryForListOfString(sql).ToList();
        }

        protected void CleanTables()
        {
            GetAllTables().Except(UndroppableTableNames).ToList().ForEach(t =>
            {
                string drop = $"DROP TABLE \"{Name}\".\"{t}\"";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }

        protected void CleanViews()
        {
            string sql = $"SELECT tbl_name FROM \"{Name}\".sqlite_master WHERE type = 'view'";
            _wrappedConnection.DbConnection.QueryForListOfString(sql).ToList().ForEach(vw =>
            {
                string drop = $"DROP VIEW \"{Name}\".\"{vw}\"";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }

        protected void CleanSequences()
        {
            string sql = $"SELECT COUNT(tbl_name) FROM \"{Name}\".sqlite_master WHERE type = 'table' AND tbl_name = 'sqlite_sequence'";
            if(_wrappedConnection.DbConnection.QueryForLong(sql) == 1)
            {
                _wrappedConnection.DbConnection.ExecuteNonQuery($"DELETE FROM \"{Name}\".sqlite_sequence", _wrappedConnection.CurrentTx);
            }
        }
    }
}
