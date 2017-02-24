using System.Collections.Generic;
using System.Linq;
using Evolve.Connection;
using Evolve.Metadata;
using Evolve.Migration;

namespace Evolve.Dialect.SQLite
{
    public class SQLiteMetadataTable : MetadataTable
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="tableName"> Metadata table name. </param>
        /// <param name="wrappedConnection"> A connection to the database. </param>
        public SQLiteMetadataTable(string tableName, WrappedConnection wrappedConnection) : base("main", tableName, wrappedConnection)
        {
        }

        /// <summary>
        ///     SQLite does not support locking. No concurrent migration supported.
        /// </summary>
        public override void Lock()
        {
        }

        public override bool IsExists()
        {
            return _wrappedConnection.QueryForLong($"SELECT COUNT(tbl_name) FROM sqlite_master WHERE type = 'table' AND tbl_name = '{TableName}'") == 1;
        }

        protected override void Create()
        {
            string sql = $"CREATE TABLE [{TableName}] " +
             "( " +
                 "id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, " +
                 "type INTEGER, " +
                 "version VARCHAR(50), " +
                 "description VARCHAR(200) NOT NULL, " +
                 "name VARCHAR(300) NOT NULL, " +
                 "checksum VARCHAR(32), " +
                 "installed_by VARCHAR(100) NOT NULL, " +
                 "installed_on TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f','now')), " +
                 "success BOOLEAN NOT NULL " +
             ")";

            _wrappedConnection.ExecuteNonQuery(sql);
        }

        protected override void InternalSave(MigrationMetadata metadata)
        {
            string sql = $"INSERT INTO [{TableName}] (type, version, description, name, checksum, installed_by, success) VALUES" +
             "( " +
                $"'{(int)metadata.Type}', " +
                $"'{metadata.Version.Label}', " +
                $"'{metadata.Description.TruncateWithEllipsis(200)}', " +
                $"'{metadata.Name.TruncateWithEllipsis(1000)}', " +
                $"'{metadata.Checksum}', " +
                $"'', " +
                $"{(metadata.Success ? 1 : 0)}" +
             ")";

            _wrappedConnection.ExecuteNonQuery(sql);
        }

        protected override IEnumerable<MigrationMetadata> InternalGetAllMetadata()
        {
            string sql = $"SELECT id, type, version, description, name, checksum, installed_by, installed_on, success FROM [{TableName}]";
            return _wrappedConnection.QueryForList(sql, r =>
            {
                return new MigrationMetadata(r.GetString(2), r.GetString(3), r.GetString(4), (MetadataType)r.GetInt16(1))
                {
                    Id = r.GetInt32(0),
                    Checksum = r.GetString(5),
                    InstalledBy = r.GetString(6),
                    InstalledOn = r.GetDateTime(7),
                    Success = r.GetBoolean(8)
                };
            });
        }
    }
}
