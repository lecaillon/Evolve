using System;
using System.Collections.Generic;
using Evolve.Connection;
using Evolve.Metadata;
using Evolve.Migration;

namespace Evolve.Dialect.SQLite
{
    public class SQLiteMetadataTable : MetadataTable
    {
        public SQLiteMetadataTable(string schema, string name, IWrappedConnection wrappedConnection) : base(schema, name, wrappedConnection)
        {
        }

        public override void Lock()
        {
            throw new NotImplementedException();
        }

        public override bool CreateIfNotExists()
        {
            if (IsExists())
            {
                return false;
            }
            else
            {
                Create();
                return true;
            }
        }

        protected override bool IsExists()
        {
            return _wrappedConnection.QueryForLong($"SELECT COUNT(tbl_name) FROM \"{Schema}\".sqlite_master WHERE type = 'table' AND tbl_name = '{Name}'") == 1;
        }

        protected override void Create()
        {
            string sql = "CREATE TABLE [{schema}].[{table}] " +
             "( " +
                 "[id] INT PRIMARY KEY AUTOINCREMENT NOT NULL, " +
                 "[version] VARCHAR(50), " +
                 "[description] VARCHAR(200) NOT NULL, " +
                 "[script] VARCHAR(1000) NOT NULL, " +
                 "[checksum] VARCHAR(32), " +
                 "[installed_by] VARCHAR(100) NOT NULL, " +
                 "[installed_on] TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f','now')), " +
                 "[success] BOOLEAN NOT NULL " +
             ")";

            _wrappedConnection.ExecuteNonQuery(sql);
        }

        protected override MigrationMetadata InternalAddMigrationMetadata(MigrationScript migration, bool success)
        {
            string sql = "INSERT INTO [{schema}].[{table}] ([version], [description], [script], [checksum], [installed_by], [success]) VALUES" +
             "( " +
                $"'{migration.Version}', " +
                $"'{migration.Description.TruncateWithEllipsis(200)}', " +
                $"'{migration.Path.TruncateWithEllipsis(1000)}', " +
                $"'{migration.CalculateChecksum()}', " +
                $"'', " +
                $"{(success ? 1 : 0)}, " +
             ")";
        }

        protected override IEnumerable<MigrationMetadata> InternalGetAllMigrationMetadata()
        {
            throw new NotImplementedException();
        }
    }
}
