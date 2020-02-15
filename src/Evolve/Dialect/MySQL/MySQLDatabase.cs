using System;
using Evolve.Connection;
using Evolve.Metadata;

namespace Evolve.Dialect.MySQL
{
    internal class MySQLDatabase : DatabaseHelper
    {
        private const string LOCK_ID = "Evolve";

        public MySQLDatabase(WrappedConnection wrappedConnection) : base(wrappedConnection)
        {
        }

        public override string DatabaseName => "MySQL";

        public override string CurrentUser => "SUBSTRING_INDEX(USER(),'@',1)";

        public override SqlStatementBuilderBase SqlStatementBuilder => new SimpleSqlStatementBuilder();

        public override string GetCurrentSchemaName() => WrappedConnection.QueryForString("SELECT DATABASE();");

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) => new MySQLMetadataTable(schema, tableName, this);

        public override Schema GetSchema(string schemaName) => new MySQLSchema(schemaName, WrappedConnection);

        public override bool TryAcquireApplicationLock() => WrappedConnection.QueryForLong($"SELECT GET_LOCK('{LOCK_ID}', 0);") == 1;

        public override bool ReleaseApplicationLock() => WrappedConnection.QueryForLong($"SELECT RELEASE_LOCK('{LOCK_ID}');") == 1;
    }
}
