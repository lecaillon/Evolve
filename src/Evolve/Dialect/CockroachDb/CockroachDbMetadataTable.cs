using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Evolve.Metadata;
using Evolve.Migration;

namespace Evolve.Dialect.CockroachDB
{
    internal class CockroachDBMetadataTable : MetadataTable
    {
        public CockroachDBMetadataTable(string schema, string tableName, DatabaseHelper database)
            : base(schema, tableName, database)
        {
        }


        /// <summary>
        ///     Implementing advisory locks in CockroachDB is being discussed, see:
        ///     https://forum.cockroachlabs.com/t/alternatives-to-pg-advisory-locks/742
        /// </summary>
        [SuppressMessage("Design", "CA1031: Do not catch general exception types")]
        protected override bool InternalTryLock()
        {
            string sqlGetLock = $"SELECT * FROM \"{Schema}\".\"{TableName}\" WHERE id = 0";
            string sqlAddLock = $"INSERT INTO \"{Schema}\".\"{TableName}\" (id, type, version, description, name, checksum, installed_by, success) " +
                                $"values(0, 0, '0', 'lock', 'lock', '', '{_database.CurrentUser}', true)";
            try
            {
                _database.WrappedConnection.BeginTransaction();
                var locks = _database.WrappedConnection.QueryForList(sqlGetLock, r => r.GetInt32(0));
                if (locks.Count() == 0)
                {
                    _database.WrappedConnection.ExecuteNonQuery(sqlAddLock);
                    _database.WrappedConnection.Commit();
                    return true;
                }
                else
                {
                    _database.WrappedConnection.Commit();
                    return false;
                }
            }
            catch
            {
                _database.WrappedConnection.TryRollback();
                return false;
            }
        }

        [SuppressMessage("Design", "CA1031: Do not catch general exception types")]
        protected override bool InternalReleaseLock()
        {
            try
            {
                _database.WrappedConnection.ExecuteNonQuery($"DELETE FROM \"{Schema}\".\"{TableName}\" WHERE id = 0");
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override bool InternalIsExists()
        {
            if (!_database.GetSchema(Schema).IsExists())
            { // database does not exist, so the metadatatable
                return false;
            }

            return _database.WrappedConnection.QueryForLong($"SELECT COUNT(*) FROM \"{Schema}\".information_schema.tables " +
                                                            $"WHERE table_catalog = '{Schema}' " +
                                                            $"AND table_schema = 'public' " +
                                                            $"AND table_name = '{TableName}'") == 1;
        }

        protected override void InternalCreate()
        {
            string sequenceName = $"{TableName}_id_seq";
            string createSequenceSql = $"CREATE SEQUENCE \"{Schema}\".\"{sequenceName}\" MAXVALUE {Int32.MaxValue};";
            string createTableSql = $"CREATE TABLE \"{Schema}\".\"{TableName}\" " +
             "( " +
                $"id INT4 PRIMARY KEY NOT NULL DEFAULT nextval('\"{Schema}\".\"{sequenceName}\"'), " +
                 "type SMALLINT, " +
                 "version VARCHAR(50), " +
                 "description VARCHAR(200) NOT NULL, " +
                 "name VARCHAR(300) NOT NULL, " +
                 "checksum VARCHAR(32), " +
                 "installed_by VARCHAR(100) NOT NULL, " +
                 "installed_on TIMESTAMP NOT NULL DEFAULT now(), " +
                 "success BOOLEAN NOT NULL " +
             ");";

            _database.WrappedConnection.ExecuteNonQuery(createSequenceSql + "\n" + createTableSql);
        }

        protected override void InternalSave(MigrationMetadata metadata)
        {
            string sql = $"INSERT INTO \"{Schema}\".\"{TableName}\" (type, version, description, name, checksum, installed_by, success) VALUES" +
             "( " +
                $"{(int)metadata.Type}, " +
                $"{(metadata.Version is null ? "null" : $"'{metadata.Version}'")}, " +
                $"'{metadata.Description.TruncateWithEllipsis(200)}', " +
                $"'{metadata.Name.TruncateWithEllipsis(1000)}', " +
                $"'{metadata.Checksum}', " +
                $"{_database.CurrentUser}, " +
                $"{(metadata.Success ? "true" : "false")}" +
             ")";

            _database.WrappedConnection.ExecuteNonQuery(sql);
        }

        protected override void InternalUpdateChecksum(int migrationId, string checksum)
        {
            string sql = $"UPDATE \"{Schema}\".\"{TableName}\" " +
                         $"SET checksum = '{checksum}' " +
                         $"WHERE id = {migrationId}";

            _database.WrappedConnection.ExecuteNonQuery(sql);
        }

        protected override IEnumerable<MigrationMetadata> InternalGetAllMetadata()
        {
            string sql = $"SELECT id, type, version, description, name, checksum, installed_by, installed_on, success FROM \"{Schema}\".\"{TableName}\"";
            return _database.WrappedConnection.QueryForList(sql, r =>
            {
                return new MigrationMetadata(r[2] as string, r.GetString(3), r.GetString(4), (MetadataType)r.GetInt16(1))
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