﻿using System;
using System.Collections.Generic;
using Evolve.Metadata;
using Evolve.Migration;

namespace Evolve.Dialect.PostgreSQL
{
    internal class PostgreSQLMetadataTable : MetadataTable
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="schema"> Existing database schema name. </param>
        /// <param name="tableName"> Metadata table name. </param>
        /// <param name="database"> A database helper used to change and restore schema of the metadata table. </param>
        public PostgreSQLMetadataTable(string schema, string tableName, DatabaseHelper database) 
            : base(schema, tableName, database)
        {
        }

        /// <summary>
        ///     Returns always true, because the lock is granted at application level.
        ///     <see cref="PostgreSQLDatabase.TryAcquireApplicationLock"/>
        /// </summary>
        protected override bool InternalTryLock() => true;

        /// <summary>
        ///     Returns always true, because the lock is released at application level.
        ///     <see cref="PostgreSQLDatabase.ReleaseApplicationLock"/>
        /// </summary>
        protected override bool InternalReleaseLock() => true;

        protected override bool InternalIsExists()
        {
            return _database.WrappedConnection.QueryForLong($"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{Schema}' AND table_name = '{TableName}'") == 1;
        }

        protected override void InternalCreate()
        {
            string sql = $"CREATE TABLE \"{Schema}\".\"{TableName}\" " +
             "( " +
                 "id SERIAL PRIMARY KEY NOT NULL, " +
                 "type SMALLINT, " +
                 "version VARCHAR(50), " +
                 "description VARCHAR(200) NOT NULL, " +
                 "name VARCHAR(300) NOT NULL, " +
                 "checksum VARCHAR(32), " +
                 "installed_by VARCHAR(100) NOT NULL, " +
                 "installed_on TIMESTAMP NOT NULL DEFAULT now(), " +
                 "success BOOLEAN NOT NULL " +
             ")";

            _database.WrappedConnection.ExecuteNonQuery(sql);
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
