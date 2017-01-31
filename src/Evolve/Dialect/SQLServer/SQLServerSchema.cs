using System.Linq;

namespace Evolve.Dialect.SQLServer
{
    public class SQLServerSchema : Schema
    {
        public SQLServerSchema(string schemaName, DatabaseHelper databaseHelper) : base(schemaName, databaseHelper)
        {
        }

        protected override bool IsExists()
        {
            string sql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{Name}'";
            return _wrappedConnection.DbConnection.QueryForLong(sql) > 0;
        }

        protected override bool IsEmpty()
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

            return _wrappedConnection.DbConnection.QueryForLong(sql) == 0;
        }

        protected override bool Create()
        {
            string sql = $"CREATE SCHEMA [{Name}]";
            _wrappedConnection.DbConnection.ExecuteNonQuery(sql, _wrappedConnection.CurrentTx);

            return true;
        }

        protected override bool Drop()
        {
            Clean();

            string sql = $"DROP SCHEMA [{Name}]";
            _wrappedConnection.DbConnection.ExecuteNonQuery(sql, _wrappedConnection.CurrentTx);

            return true;
        }

        protected override bool Clean()
        {
            CleanForeignKeys();
            CleanDefaultConstraints();
            CleanProcedures();
            CleanViews();
            CleanTables();
            CleanFunctions();
            CleanTypes();
            CleanSynonyms();
            CleanSequences(); // SQLServer >= 11

            return true;
        }

        private void CleanForeignKeys()
        {
            string sql = "SELECT TABLE_NAME, CONSTRAINT_NAME " + 
                         "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS " +
                         "WHERE CONSTRAINT_TYPE IN ('FOREIGN KEY','CHECK') " +
                        $"AND TABLE_SCHEMA = '{Name}'";

            _wrappedConnection.DbConnection.QueryForListOfT(sql, (r) => new { Table = r.GetString(0), Constraint = r.GetString(1) }).ToList().ForEach(x =>
            {
                string drop = $"ALTER TABLE [{Name}].[{x.Table}] DROP CONSTRAINT [{x.Constraint}]";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }

        private void CleanDefaultConstraints()
        {
            string sql = "SELECT t.name as TABLE_NAME, d.name as CONSTRAINT_NAME " +
                         "FROM sys.tables t " +
                         "INNER JOIN sys.default_constraints d ON d.parent_object_id = t.object_id " +
                         "INNER JOIN sys.schemas s ON s.schema_id = t.schema_id " +
                        $"WHERE s.name = '{Name}'";

            _wrappedConnection.DbConnection.QueryForListOfT(sql, (r) => new { Table = r.GetString(0), Constraint = r.GetString(1) }).ToList().ForEach(x =>
            {
                string drop = $"ALTER TABLE [{Name}].[{x.Table}] DROP CONSTRAINT [{x.Constraint}]";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }

        private void CleanProcedures()
        {
            string sql = $"SELECT routine_name FROM INFORMATION_SCHEMA.ROUTINES WHERE routine_schema = '{Name}' AND routine_type = 'PROCEDURE' ORDER BY created DESC";
            _wrappedConnection.DbConnection.QueryForListOfString(sql).ToList().ForEach(proc =>
            {
                string drop = $"DROP PROCEDURE [{Name}].[{proc}]";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }

        private void CleanViews()
        {
            string sql = $"SELECT table_name FROM INFORMATION_SCHEMA.VIEWS WHERE table_schema = '{Name}'";
            _wrappedConnection.DbConnection.QueryForListOfString(sql).ToList().ForEach(vw =>
            {
                string drop = $"DROP VIEW [{Name}].[{vw}]";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }

        private void CleanTables()
        {
            string sql = $"SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_type='BASE TABLE' AND table_schema = '{Name}'";
            _wrappedConnection.DbConnection.QueryForListOfString(sql).ToList().ForEach(t =>
            {
                string drop = $"DROP TABLE [{Name}].[{t}]";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }

        private void CleanFunctions()
        {
            string sql = $"SELECT routine_name FROM INFORMATION_SCHEMA.ROUTINES WHERE routine_schema = '{Name}' AND routine_type = 'FUNCTION' ORDER BY created DESC";
            _wrappedConnection.DbConnection.QueryForListOfString(sql).ToList().ForEach(fn =>
            {
                string drop = $"DROP FUNCTION [{Name}].[{fn}]";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }

        private void CleanTypes()
        {
            string sql = $"SELECT t.name FROM sys.types t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.is_user_defined = 1 AND s.name = '{Name}'";
            _wrappedConnection.DbConnection.QueryForListOfString(sql).ToList().ForEach(t =>
            {
                string drop = $"DROP TYPE [{Name}].[{t}]";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }

        private void CleanSynonyms()
        {
            string sql = $"SELECT sn.name FROM sys.synonyms sn INNER JOIN sys.schemas s ON sn.schema_id = s.schema_id WHERE s.name = '{Name}'";
            _wrappedConnection.DbConnection.QueryForListOfString(sql).ToList().ForEach(s =>
            {
                string drop = $"DROP SYNONYM [{Name}].[{s}]";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }

        private void CleanSequences()
        {
            string sqlversion = "SELECT CAST (CASE WHEN CAST(SERVERPROPERTY ('productversion') as VARCHAR) LIKE '8%' THEN 8 " +
                                                  "WHEN CAST(SERVERPROPERTY ('productversion') as VARCHAR) LIKE '9%' THEN 9 " +
                                                  "WHEN CAST(SERVERPROPERTY ('productversion') as VARCHAR) LIKE '10%' THEN 10 " +
                                                  "ELSE CAST(LEFT(CAST(SERVERPROPERTY ('productversion') as VARCHAR), 2) as int) " +
                                             "END AS int)";

            if (_wrappedConnection.DbConnection.QueryForLong(sqlversion) < 11)
                return;

            string sql = $"SELECT sequence_name FROM INFORMATION_SCHEMA.SEQUENCES WHERE sequence_schema = '{Name}'";
            _wrappedConnection.DbConnection.QueryForListOfString(sql).ToList().ForEach(s =>
            {
                string drop = $"DROP SEQUENCE [{Name}].[{s}]";
                _wrappedConnection.DbConnection.ExecuteNonQuery(drop, _wrappedConnection.CurrentTx);
            });
        }
    }
}
