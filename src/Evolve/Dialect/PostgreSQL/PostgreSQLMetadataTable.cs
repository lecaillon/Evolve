using System.Collections.Generic;
using System.Linq;
using Evolve.Connection;
using Evolve.Metadata;
using Evolve.Migration;

namespace Evolve.Dialect.PostgreSQL
{
    public class PostgreSQLMetadataTable : MetadataTable
    {
        public PostgreSQLMetadataTable(string schema, string tableName, WrappedConnection wrappedConnection) : base(schema, tableName, wrappedConnection)
        {
        }

        public override void Lock()
        {
            _wrappedConnection.ExecuteNonQuery($"SELECT * FROM '{TableName}' FOR UPDATE");
        }

        protected override bool IsExists()
        {
            return _wrappedConnection.QueryForLong($"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{Schema}' AND table_name = '{TableName}'") == 1;
        }

        protected override void Create()
        {
            string sql = $"CREATE TABLE \"{TableName}\" " +
             "( " +
                 "id SERIAL PRIMARY KEY NOT NULL, " +
                 "version VARCHAR(50), " +
                 "description VARCHAR(200) NOT NULL, " +
                 "name VARCHAR(1000) NOT NULL, " +
                 "checksum VARCHAR(32), " +
                 "installed_by VARCHAR(100) NOT NULL, " +
                 "installed_on TIMESTAMP NOT NULL DEFAULT now(), " +
                 "success BOOLEAN NOT NULL " +
             ")";

            _wrappedConnection.ExecuteNonQuery(sql);
        }

        protected override void InternalAddMigrationMetadata(MigrationScript migration, bool success)
        {
            string sql = $"INSERT INTO \"{TableName}\" (version, description, name, checksum, installed_by, success) VALUES" +
             "( " +
                $"'{migration.Version}', " +
                $"'{migration.Description.TruncateWithEllipsis(200)}', " +
                $"'{migration.Path.TruncateWithEllipsis(1000)}', " +
                $"'{migration.CalculateChecksum()}', " +
                $"'', " +
                $"{(success ? "true" : "false")}" +
             ")";

            _wrappedConnection.ExecuteNonQuery(sql);
        }

        protected override IEnumerable<MigrationMetadata> InternalGetAllMigrationMetadata()
        {
            string sql = $"SELECT id, version, description, name, checksum, installed_by, installed_on, success FROM \"{TableName}\"";
            return _wrappedConnection.QueryForList(sql, r =>
            {
                return new MigrationMetadata(r.GetString(1), r.GetString(2), r.GetString(3))
                {
                    Id = r.GetInt32(0),
                    Checksum = r.GetString(4),
                    InstalledBy = r.GetString(5),
                    InstalledOn = r.GetDateTime(6),
                    Success = r.GetBoolean(7)
                };
            })
            .DefaultIfEmpty();
        }
    }
}
