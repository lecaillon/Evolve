﻿using System.Linq;
using Evolve.Connection;

namespace Evolve.Dialect.PostgreSQL
{
    internal class PostgreSQLSchema : Schema
    {
        public PostgreSQLSchema(string schemaName, WrappedConnection wrappedConnection) : base(schemaName, wrappedConnection)
        {
            Version = _wrappedConnection.QueryForLong("SHOW server_version_num;");
        }

        public long Version { get; }

        public override bool IsExists()
        {
            string sql = $"SELECT COUNT(*) FROM pg_namespace WHERE nspname = '{Name}'";
            return _wrappedConnection.QueryForLong(sql) > 0;
        }

        public override bool IsEmpty()
        {
            string sql = $"SELECT EXISTS " +
                          "(" +
                          "  SELECT c.oid " +
                          "  FROM pg_catalog.pg_class c " +
                          "  JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace " +
                          "  LEFT JOIN pg_catalog.pg_depend d ON d.objid = c.oid AND d.deptype = 'e' " +
                         $"  WHERE  n.nspname = '{Name}' AND d.objid IS NULL AND c.relkind IN ('r', 'v', 'S', 't') " +
                          " UNION ALL " +
                          "  SELECT t.oid " +
                          "  FROM pg_catalog.pg_type t " +
                          "  JOIN pg_catalog.pg_namespace n ON n.oid = t.typnamespace " +
                          "  LEFT JOIN pg_catalog.pg_depend d ON d.objid = t.oid AND d.deptype = 'e' " +
                         $"  WHERE n.nspname = '{Name}' AND d.objid IS NULL AND t.typcategory NOT IN ('A', 'C') " +
                          " UNION ALL " +
                          "  SELECT p.oid " +
                          "  FROM pg_catalog.pg_proc p " +
                          "  JOIN pg_catalog.pg_namespace n ON n.oid = p.pronamespace " +
                          "  LEFT JOIN pg_catalog.pg_depend d ON d.objid = p.oid AND d.deptype = 'e' " +
                         $"  WHERE n.nspname = '{Name}' AND d.objid IS NULL " +
                         $")";

            return !_wrappedConnection.QueryForBool(sql);
        }

        public override bool Create()
        {
            _wrappedConnection.ExecuteNonQuery($"CREATE SCHEMA \"{Name}\"");

            return true;
        }

        public override bool Drop()
        {
            _wrappedConnection.ExecuteNonQuery($"DROP SCHEMA IF EXISTS \"{Name}\" CASCADE");

            return true;
        }

        public override bool Erase()
        {
            DropMaterializedViews(); // PostgreSQL >= 9.3
            DropViews();
            DropTables();
            DropBaseTypes(true);
            DropRoutines();
            DropEnums();
            DropDomains();
            DropSequences();
            DropBaseAggregates(); // PostgreSQL < 11
            DropBaseTypes(false);

            return true;
        }

        protected void DropMaterializedViews()
        {
            if (Version < 90300)
            {
                return;
            }

            string sql = $"SELECT relname FROM pg_catalog.pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace WHERE c.relkind = 'm' AND n.nspname = '{Name}'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(view =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP MATERIALIZED VIEW IF EXISTS \"{Name}\".\"{Quote(view)}\" CASCADE");
            });
        }

        protected void DropSequences()
        {
            string sql = $"SELECT sequence_name FROM information_schema.sequences WHERE sequence_schema = '{Name}'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(seq =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP SEQUENCE IF EXISTS \"{Name}\".\"{Quote(seq)}\"");
            });
        }

        protected void DropBaseTypes(bool recreate)
        {
            string sql = "SELECT typname, typcategory " +
                         "FROM pg_catalog.pg_type t " +
                         "WHERE (t.typrelid = 0 OR (SELECT c.relkind = 'c' FROM pg_catalog.pg_class c WHERE c.oid = t.typrelid)) " +
                         "AND NOT EXISTS(SELECT 1 FROM pg_catalog.pg_type el WHERE el.oid = t.typelem AND el.typarray = t.oid) " +
                        $"AND t.typnamespace in (SELECT oid FROM pg_catalog.pg_namespace WHERE nspname = '{Name}')";

            _wrappedConnection.QueryForList(sql, r => new { TypeName = r.GetString(0), TypeCategory = r.GetChar(1) }).ToList().ForEach(x =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP TYPE IF EXISTS \"{Name}\".\"{Quote(x.TypeName)}\" CASCADE");
            });

            if (recreate)
            {
                _wrappedConnection.QueryForList(sql, r => new { TypeName = r.GetString(0), TypeCategory = r.GetChar(1) }).ToList().ForEach(x =>
                {
                    // Only recreate Pseudo-types (P) and User-defined types (U)
                    if (x.TypeCategory == 'P' || x.TypeCategory == 'U')
                    {
                        _wrappedConnection.ExecuteNonQuery($"CREATE TYPE \"{Name}\".\"{Quote(x.TypeName)}\"");
                    }
                });
            }
        }

        protected void DropBaseAggregates()
        {
            if (Version >= 110000)
            {
                return;
            }

            string sql = "SELECT proname, oidvectortypes(proargtypes) AS args " +
                         "FROM pg_proc INNER JOIN pg_namespace ns ON (pg_proc.pronamespace = ns.oid) " +
                        $"WHERE pg_proc.proisagg = true AND ns.nspname = '{Name}'";

            _wrappedConnection.QueryForList(sql, r => new { ProName = r.GetString(0), Args = r.GetString(1) }).ToList().ForEach(x =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP AGGREGATE IF EXISTS \"{Name}\".\"{Quote(x.ProName)}\" ({x.Args}) CASCADE");
            });
        }


        protected void DropRoutines()
        {
            if (Version < 110000)
            {
                string sql = "SELECT proname, oidvectortypes(proargtypes) AS args " +
                             "FROM pg_proc INNER JOIN pg_namespace ns ON (pg_proc.pronamespace = ns.oid) " +
                             "LEFT JOIN pg_depend dep ON dep.objid = pg_proc.oid AND dep.deptype = 'e' " +
                            $"WHERE pg_proc.proisagg = false AND ns.nspname = '{Name}' AND dep.objid IS NULL";

                _wrappedConnection.QueryForList(sql, r => new { ProName = r.GetString(0), Args = r.GetString(1) }).ToList().ForEach(x =>
                {
                    _wrappedConnection.ExecuteNonQuery($"DROP FUNCTION IF EXISTS \"{Name}\".\"{Quote(x.ProName)}\" ({x.Args}) CASCADE");
                });
            }
            else
            {
                string sql = "SELECT proname, oidvectortypes(proargtypes) AS args, pg_proc.prokind AS kind " +
                             "FROM pg_proc INNER JOIN pg_namespace ns ON (pg_proc.pronamespace = ns.oid) " +
                             "LEFT JOIN pg_depend dep ON dep.objid = pg_proc.oid AND dep.deptype = 'e' " +
                            $"WHERE ns.nspname = '{Name}' AND dep.objid IS NULL";

                _wrappedConnection.QueryForList(sql, r => new { ProName = r.GetString(0), Args = r.GetString(1), Kind = r.GetChar(2) }).ToList().ForEach(x =>
                {
                    string? objectType = null;
                    switch (x.Kind)
                    {
                        case 'a': objectType = "AGGREGATE"; break;
                        case 'f': objectType = "FUNCTION"; break;
                        case 'p': objectType = "PROCEDURE"; break;
                        default: break;
                    }

                    if (objectType is null)
                    {
                        return;
                    }

                    _wrappedConnection.ExecuteNonQuery($"DROP {objectType} IF EXISTS \"{Name}\".\"{Quote(x.ProName)}\" ({x.Args}) CASCADE");
                });
            }
        }

        protected void DropEnums()
        {
            string sql = $"SELECT t.typname FROM pg_catalog.pg_type t INNER JOIN pg_catalog.pg_namespace n ON n.oid = t.typnamespace WHERE n.nspname = '{Name}' and t.typtype = 'e'";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(enumName =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP TYPE \"{Name}\".\"{Quote(enumName)}\"");
            });
        }

        protected void DropDomains()
        {
            string sql = "SELECT t.typname " +
                         "FROM pg_catalog.pg_type t " +
                         "LEFT JOIN pg_catalog.pg_namespace n ON n.oid = t.typnamespace " +
                         "LEFT JOIN pg_depend dep ON dep.objid = t.oid AND dep.deptype = 'e' " +
                         "WHERE t.typtype = 'd' " +
                        $"AND n.nspname = '{Name}' " +
                         "AND dep.objid IS NULL";
            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(domain =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP DOMAIN \"{Name}\".\"{Quote(domain)}\"");
            });
        }

        protected void DropViews()
        {
            string sql = "SELECT relname " +
                         "FROM pg_catalog.pg_class c " +
                         "JOIN pg_namespace n ON n.oid = c.relnamespace " +
                         "LEFT JOIN pg_depend dep ON dep.objid = c.oid AND dep.deptype = 'e' " +
                        $"WHERE c.relkind = 'v' AND  n.nspname = '{Name}' AND dep.objid IS NULL";

            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(view =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP VIEW IF EXISTS \"{Name}\".\"{Quote(view)}\" CASCADE");
            });
        }

        protected void DropTables()
        {
            string sql = "SELECT t.table_name " +
                         "FROM information_schema.tables t " +
                         "LEFT JOIN pg_depend dep ON dep.objid = (quote_ident(t.table_schema)||'.'||quote_ident(t.table_name))::regclass::oid AND dep.deptype = 'e' " +
                        $"WHERE table_schema = '{Name}' " +
                         "AND table_type='BASE TABLE' " +
                         "AND dep.objid IS NULL " +
                         "AND NOT (SELECT EXISTS (SELECT inhrelid FROM pg_catalog.pg_inherits WHERE inhrelid = (quote_ident(t.table_schema)||'.'||quote_ident(t.table_name))::regclass::oid))";

            _wrappedConnection.QueryForListOfString(sql).ToList().ForEach(table =>
            {
                _wrappedConnection.ExecuteNonQuery($"DROP TABLE IF EXISTS \"{Name}\".\"{Quote(table)}\" CASCADE");
            });
        }

        private static string Quote(string dbObject) => dbObject.Replace("\"", "\"\"");
    }
}
