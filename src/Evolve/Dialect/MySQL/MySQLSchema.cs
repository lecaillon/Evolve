using System;
using System.Linq;
using Evolve.Connection;

namespace Evolve.Dialect.MySQL
{
    public class MySQLSchema : Schema
    {
        public MySQLSchema(string schemaName, WrappedConnection wrappedConnection) : base(schemaName, wrappedConnection)
        {
        }

        public override bool IsExists()
        {
            string sql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{Name}'";
            return _wrappedConnection.QueryForLong(sql) > 0;
        }

        public override bool IsEmpty()
        {
            string sql = "SELECT " +
                $"(SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{Name}') + " +
                $"(SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = '{Name}') + " +
                $"(SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_SCHEMA = '{Name}') + " +
                $"(SELECT COUNT(*) FROM INFORMATION_SCHEMA.EVENTS WHERE EVENT_SCHEMA = '{Name}') + " +
                $"(SELECT COUNT(*) FROM INFORMATION_SCHEMA.TRIGGERS WHERE TRIGGER_SCHEMA = '{Name}') + " +
                $"(SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '{Name}')";

            return _wrappedConnection.QueryForLong(sql) == 0;
        }

        public override bool Create()
        {
            _wrappedConnection.ExecuteNonQuery($"CREATE SCHEMA `{Name}`");

            return true;
        }

        public override bool Drop()
        {
            _wrappedConnection.ExecuteNonQuery($"DROP SCHEMA `{Name}`");

            return true;
        }

        public override bool Erase()
        {
            DropEvents();
            DropRoutines();
            DropViews();
            DropTables();

            return true;
        }

        private void DropTables()
        {
            _wrappedConnection.ExecuteNonQuery($"SET FOREIGN_KEY_CHECKS = 0");

            string sql = $"SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_type='BASE TABLE' AND table_schema = '{Name}'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(t =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP TABLE `{Name}`.`{t}`");
            });

            _wrappedConnection.ExecuteNonQuery($"SET FOREIGN_KEY_CHECKS = 1");
        }

        private void DropViews()
        {
            string sql = $"SELECT table_name FROM information_schema.views WHERE table_schema = '{Name}'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(vw =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP VIEW `{Name}`.`{vw}`");
            });
        }

        private void DropRoutines()
        {
            string sql = $"SELECT routine_name, routine_type FROM information_schema.routines WHERE routine_schema = '{Name}'";

            _wrappedConnection.QueryForList(sql, (r) => new { RoutineName = r.GetString(0), RoutineType = r.GetString(1) }).ToList().ForEach(x =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP {x.RoutineType} `{Name}`.`{x.RoutineName}`");
            });
        }

        private void DropEvents()
        {
            string sql = $"SELECT event_name FROM information_schema.events WHERE event_schema = '{Name}'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(evt =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP EVENT `{Name}`.`{evt}`");
            });
        }
    }
}
