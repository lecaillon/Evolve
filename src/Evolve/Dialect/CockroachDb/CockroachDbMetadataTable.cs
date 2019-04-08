using System;
using Evolve.Dialect.PostgreSQL;

namespace Evolve.Dialect.CockroachDb
{
    public class CockroachDbMetadataTable : PostgreSQLMetadataTable
    {
        public CockroachDbMetadataTable(string schema, string tableName, DatabaseHelper database) : base(schema,
            tableName, database)
        {
        }

        protected override void InternalCreate()
        {
            var sequenceName = $"{TableName}_id_seq";

            string createSequenceSql = $"CREATE SEQUENCE \"{sequenceName}\" MAXVALUE {Int32.MaxValue};";

            string createTableSql = $"CREATE TABLE \"{Schema}\".\"{TableName}\" " +
                         "( " +
                         $"id INT4 PRIMARY KEY NOT NULL DEFAULT nextval('{sequenceName}'), " +
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
    }
}