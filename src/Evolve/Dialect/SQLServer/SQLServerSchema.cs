using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Dialect.SQLServer
{
    public class SQLServerSchema : Schema
    {
        public SQLServerSchema(string schemaName, DatabaseHelper databaseHelper) : base(schemaName, databaseHelper)
        {
        }

        protected override bool InternalIsExists()
        {
            // SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME=?
            throw new NotImplementedException();
        }

        protected override bool InternalClean()
        {
            throw new NotImplementedException();
        }

        protected override bool InternalCreate()
        {
            throw new NotImplementedException();
        }

        protected override bool InternalDrop()
        {
            throw new NotImplementedException();
        }

        protected override bool InternalIsEmpty()
        {
            throw new NotImplementedException();
        }
    }
}
