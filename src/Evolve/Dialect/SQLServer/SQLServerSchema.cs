using System.Linq;
using Evolve.Connection;

namespace Evolve.Dialect.SQLServer
{
    public class SQLServerSchema : Schema
    {
        public SQLServerSchema(string schemaName, WrappedConnection wrappedConnection) : base(schemaName, wrappedConnection)
        {
        }

        public override bool IsExists()
        {
            string sql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{Name}'";
            return _wrappedConnection.QueryForLong(sql) > 0;
        }

        public override bool IsEmpty()
        {
            string sql = "SELECT COUNT(*) FROM " +
                         "( " +
                             "SELECT TABLE_NAME as OBJECT_NAME, TABLE_SCHEMA as OBJECT_SCHEMA FROM INFORMATION_SCHEMA.TABLES " +
                             "UNION " +
                             "SELECT TABLE_NAME as OBJECT_NAME, TABLE_SCHEMA as OBJECT_SCHEMA FROM INFORMATION_SCHEMA.VIEWS " +
                             "UNION " +
                             "SELECT CONSTRAINT_NAME as OBJECT_NAME, TABLE_SCHEMA as OBJECT_SCHEMA FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS " +
                             "UNION " +
                             "SELECT ROUTINE_NAME as OBJECT_NAME, ROUTINE_SCHEMA as OBJECT_SCHEMA FROM INFORMATION_SCHEMA.ROUTINES " +
                         ") x " +
                        $"WHERE OBJECT_SCHEMA = '{Name}'";

            return _wrappedConnection.QueryForLong(sql) == 0;
        }

        public override bool Create()
        {
            _wrappedConnection.ExecuteNonQuery($"CREATE SCHEMA [{Name}]");

            return true;
        }

        public override bool Drop()
        {
            Erase(); // ?

            _wrappedConnection.ExecuteNonQuery($"DROP SCHEMA [{Name}]");

            return true;
        }

        public override bool Erase()
        {
            DropForeignKeys();
            DropDefaultConstraints();
            DropProcedures();
            DropViews();
            DropTables();
            DropFunctions();
            DropTypes();
            DropSynonyms();
            DropSequences(); // SQLServer >= 11

            return true;
        }

        protected void DropForeignKeys()
        {
            string sql = "SELECT TABLE_NAME, CONSTRAINT_NAME " + 
                         "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS " +
                         "WHERE CONSTRAINT_TYPE IN ('FOREIGN KEY','CHECK') " +
                        $"AND TABLE_SCHEMA = '{Name}'";

            _wrappedConnection.QueryForList(sql, (r) => new { RoutineName = r.GetString(0), RoutineType = r.GetString(1) }).ToList().ForEach(x =>
            {
                _wrappedConnection.ExecuteNonQuery($"ALTER TABLE [{Name}].[{x.RoutineName}] DROP CONSTRAINT [{x.RoutineType}]");
            });
        }

        protected void DropDefaultConstraints()
        {
            string sql = "SELECT t.name as TABLE_NAME, d.name as CONSTRAINT_NAME " +
                         "FROM sys.tables t " +
                         "INNER JOIN sys.default_constraints d ON d.parent_object_id = t.object_id " +
                         "INNER JOIN sys.schemas s ON s.schema_id = t.schema_id " +
                        $"WHERE s.name = '{Name}'";

            _wrappedConnection.QueryForList(sql, (r) => new { RoutineName = r.GetString(0), RoutineType = r.GetString(1) }).ToList().ForEach(x =>
            {
                _wrappedConnection.ExecuteNonQuery($"ALTER TABLE [{Name}].[{x.RoutineName}] DROP CONSTRAINT [{x.RoutineType}]");
            });
        }

        protected void DropProcedures()
        {
            string sql = $"SELECT routine_name FROM INFORMATION_SCHEMA.ROUTINES WHERE routine_schema = '{Name}' AND routine_type = 'PROCEDURE' ORDER BY created DESC";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(proc =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP PROCEDURE [{Name}].[{proc}]");
            });
        }

        protected void DropViews()
        {
            string sql = $"SELECT table_name FROM INFORMATION_SCHEMA.VIEWS WHERE table_schema = '{Name}'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(vw =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP VIEW [{Name}].[{vw}]");
            });
        }

        protected void DropTables()
        {
            string sql = $"SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_type='BASE TABLE' AND table_schema = '{Name}'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(t =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP TABLE [{Name}].[{t}]");
            });
        }

        protected void DropFunctions()
        {
            string sql = $"SELECT routine_name FROM INFORMATION_SCHEMA.ROUTINES WHERE routine_schema = '{Name}' AND routine_type = 'FUNCTION' ORDER BY created DESC";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(fn =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP FUNCTION [{Name}].[{fn}]");
            });
        }

        protected void DropTypes()
        {
            string sql = $"SELECT t.name FROM sys.types t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.is_user_defined = 1 AND s.name = '{Name}'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(t =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP TYPE [{Name}].[{t}]");
            });
        }

        protected void DropSynonyms()
        {
            string sql = $"SELECT sn.name FROM sys.synonyms sn INNER JOIN sys.schemas s ON sn.schema_id = s.schema_id WHERE s.name = '{Name}'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(s =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP SYNONYM [{Name}].[{s}]");
            });
        }

        protected void DropSequences()
        {
            string sqlversion = "SELECT CAST (CASE WHEN CAST(SERVERPROPERTY ('productversion') as VARCHAR) LIKE '8%' THEN 8 " +
                                                  "WHEN CAST(SERVERPROPERTY ('productversion') as VARCHAR) LIKE '9%' THEN 9 " +
                                                  "WHEN CAST(SERVERPROPERTY ('productversion') as VARCHAR) LIKE '10%' THEN 10 " +
                                                  "ELSE CAST(LEFT(CAST(SERVERPROPERTY ('productversion') as VARCHAR), 2) as int) " +
                                             "END AS int)";

            if (_wrappedConnection.QueryForLong(sqlversion) < 11)
                return;

            string sql = $"SELECT sequence_name FROM INFORMATION_SCHEMA.SEQUENCES WHERE sequence_schema = '{Name}'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(s =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP SEQUENCE [{Name}].[{s}]");
            });
        }
    }
}
