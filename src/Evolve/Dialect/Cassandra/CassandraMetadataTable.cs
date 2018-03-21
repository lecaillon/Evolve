﻿using System;
using System.Collections.Generic;
using Evolve.Metadata;
using Evolve.Migration;

namespace Evolve.Dialect.Cassandra
{
    public sealed class CassandraMetadataTable : MetadataTable
    {
        public CassandraMetadataTable(string schema, string tableName, DatabaseHelper database)
            : base(schema, tableName, database)
        {
        }

        protected override void InternalCreate() =>
            _database.WrappedConnection.ExecuteNonQuery(
                $"create table if not exists {Schema}.{TableName} ( " +
                   "id int primary key, " +
                   "type tinyint, " +
                   "version text, " +
                   "description text, " +
                   "name text, " +
                   "checksum text, " +
                   "installed_by text, " +
                   "installed_on timestamp, " +
                   "success boolean )");

        protected override IEnumerable<MigrationMetadata> InternalGetAllMetadata()
        {
            string cql = $"SELECT id, type, version, description, name, checksum, installed_by, installed_on, success FROM {Schema}.{TableName}";

            return _database.WrappedConnection.QueryForList(cql, r =>
            {
                return new MigrationMetadata(r.GetString(2), r.GetString(3), r.GetString(4), (MetadataType)(sbyte)r.GetValue(1))
                {
                    Id = r.GetInt32(0),
                    Checksum = r.GetString(5),
                    InstalledBy = r.GetString(6),
                    InstalledOn = ((DateTimeOffset)r.GetValue(7)).DateTime,
                    Success = r.GetBoolean(8)
                };
            });
        }

        protected override bool InternalIsExists() =>
            _database.WrappedConnection.QueryForLong(
                $"select count(table_name) from system_schema.tables where keyspace_name = '{Schema}' and table_name = '{TableName}'") > 0;

        //No locks in Cassandra
        protected override void InternalLock() { }

        protected override void InternalSave(MigrationMetadata metadata)
        {
            //Cassandra does not support auto incremented IDs, so we'll insert a random
            //Using Guid.GetHashCode ensure low probability of collision, and checking that it does not exist yet lowers it even more
            //It might still theoretically happen that the ID already exists, but in that case retrying the operation will likely fix the issue
            do
            {
                metadata.Id = Math.Abs(Guid.NewGuid().GetHashCode());
            }
            while (metadata.Id == 0 || idExists(metadata.Id));

            _database.WrappedConnection.ExecuteNonQuery(
                $"insert into {Schema}.{TableName} (id, type, version, description, name, checksum, installed_by, installed_on, success) " +
                $"values({metadata.Id}, {(int)metadata.Type}, '{metadata.Version}', '{metadata.Description}', '{metadata.Name}', '{metadata.Checksum}', 'anonymous', toUnixTimestamp(now()), {metadata.Success})");

            bool idExists(int id) =>
                _database.WrappedConnection.QueryForLong($"select count(id) from {Schema}.{TableName} where id = {id}") > 0;
        }

        protected override void InternalUpdateChecksum(int migrationId, string checksum) =>
            _database.WrappedConnection.ExecuteNonQuery(
                $"update {Schema}.{TableName} " +
                $"set checksum = '{checksum}' " +
                $"where id = {migrationId}");
    }
}
