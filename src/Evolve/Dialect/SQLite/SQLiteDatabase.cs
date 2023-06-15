using EvolveDb.Connection;
using EvolveDb.Metadata;

namespace EvolveDb.Dialect.SQLite
{
    internal class SQLiteDatabase : DatabaseHelper
    {
        public SQLiteDatabase(WrappedConnection wrappedConnection) : base(wrappedConnection)
        {
        }

        public override string DatabaseName => "SQLite";

        public override string CurrentUser => "''";

        public override SqlStatementBuilderBase SqlStatementBuilder => new SimpleSqlStatementBuilder();


        public override string GetCurrentSchemaName() => "main";

        public override Schema GetSchema(string schemaName) => new SQLiteSchema(WrappedConnection);

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) => new SQLiteMetadataTable(tableName, this);

        /// <summary>
        ///     Not supported in SQLite.
        /// </summary>
        /// <returns> Always true </returns>
        public override bool TryAcquireApplicationLock(object? lockId = null) => true;

        /// <summary>
        ///     Not supported in SQLite.
        /// </summary>
        /// <returns> Always true </returns>
        public override bool ReleaseApplicationLock(object? lockId = null) => true;
    }
}
