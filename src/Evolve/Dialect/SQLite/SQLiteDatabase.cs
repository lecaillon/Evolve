﻿using Evolve.Connection;
using Evolve.Metadata;

namespace Evolve.Dialect.SQLite
{
    public class SQLiteDatabase : DatabaseHelper
    {
        public SQLiteDatabase(WrappedConnection wrappedConnection) : base(wrappedConnection)
        {
        }

        public override string DatabaseName => "SQLite";

        public override string CurrentUser => "''";

        public override SqlStatementBuilderBase SqlStatementBuilder => new SimpleSqlStatementBuilder();


        public override string GetCurrentSchemaName() => "main";

        public override Schema GetSchema(string schemaName) => new SQLiteSchema(WrappedConnection);

        /// <summary>
        ///     SQLite does not support setting the schema.
        /// </summary>
        protected override void InternalChangeSchema(string toSchemaName) { }

        public override IEvolveMetadata GetMetadataTable(string schema, string tableName) => new SQLiteMetadataTable(tableName, this);

        /// <summary>
        ///     Not supported in SQLite.
        /// </summary>
        /// <returns> Always true </returns>
        public override bool TryAcquireApplicationLock() => true;

        /// <summary>
        ///     Not supported in SQLite.
        /// </summary>
        /// <returns> Always true </returns>
        public override bool ReleaseApplicationLock() => true;
    }
}
