using System.Collections.Generic;
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
        /// <param name="database"> A database helper used to change and restore schema of the metadata table. </param>
        public SQLiteMetadataTable(string tableName, DatabaseHelper database) : base("main", tableName, database)
        {
        }

        /// <summary>
        ///     SQLite does not support locking. No concurrent migration supported.
        /// </summary>
        protected override void InternalLock()
        {
        }

        protected override bool InternalIsExists()
        {
            return _database.WrappedConnection.QueryForLong($"SELECT COUNT(tbl_name) FROM sqlite_master WHERE type = 'table' AND tbl_name = '{TableName}'") == 1;
        }

        protected override void InternalCreate()
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

            _database.WrappedConnection.ExecuteNonQuery(sql);
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
                $"{_database.CurrentUser}, " +
                $"{(metadata.Success ? 1 : 0)}" +
             ")";

            _database.WrappedConnection.ExecuteNonQuery(sql);
        }

        protected override void InternalUpdateChecksum(int migrationId, string checksum)
        {
            string sql = $"UPDATE [{TableName}] " +
                         $"SET checksum = '{checksum}' " +
                         $"WHERE id = {migrationId}";

            _database.WrappedConnection.ExecuteNonQuery(sql);
        }

        protected override IEnumerable<MigrationMetadata> InternalGetAllMetadata()
        {
            string sql = $"SELECT id, type, version, description, name, checksum, installed_by, installed_on, success FROM [{TableName}]";
            return _database.WrappedConnection.QueryForList(sql, r =>
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
