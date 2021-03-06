using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Metadata;
using Evolve.Tests.Infrastructure;
using Xunit.Abstractions;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration
{
    public abstract class Scenario<T> where T : IDbContainerFixture
    {
        protected readonly T _dbContainer;
        protected readonly ITestOutputHelper _output;

        public Scenario(T dbContainer, ITestOutputHelper output)
        {
            _dbContainer = dbContainer;
            _output = output;

            if (Local)
            {
                dbContainer.Run(fromScratch: true);
            }

            if (Dbms == DBMS.SQLServer)
            {
                TestUtil.CreateSqlServerDatabase(DbName, (_dbContainer as SQLServerFixture).GetCnxStr("master"));
                Cnn = new SqlConnection((_dbContainer as SQLServerFixture).GetCnxStr(DbName));
            }
            else
            {
                Cnn = _dbContainer.CreateDbConnection();
            }

            WrappedConnection = new WrappedConnection(Cnn);
            Evolve = new Evolve(Cnn, msg => _output.WriteLine(msg))
            {
                Schemas = new[] { SchemaName },
                MetadataTableSchema = SchemaName,
                Locations = new[] { ScenarioFolder },
                Placeholders = new() { ["${db}"] = DbName, ["${schema}"] = SchemaName },
            };
        }

        public DbConnection Cnn { get; }
        internal WrappedConnection WrappedConnection { get; }
        public Evolve Evolve { get; }
        public DBMS Dbms => typeof(T).Name switch
        {
            "CassandraFixture" => DBMS.Cassandra,
            "CockroachDbFixture" => DBMS.CockroachDB,
            "MySQLFixture" => DBMS.MySQL,
            "PostgreSqlFixture" => DBMS.PostgreSQL,
            "SQLServerFixture" => DBMS.SQLServer,
            _ => throw new NotSupportedException($"{typeof(T).Name} not supported.")
        };
        public string DbName => Dbms == DBMS.SQLServer ? GetType().Name : "";
        public string SchemaName => Dbms == DBMS.SQLServer ? "dbo" : GetType().Name.ToLower();
        internal DatabaseHelper DbHelper => Dbms switch
        {
            DBMS.Cassandra => DatabaseHelperFactory.GetDatabaseHelper(DBMS.Cassandra, WrappedConnection),
            DBMS.CockroachDB => DatabaseHelperFactory.GetDatabaseHelper(DBMS.CockroachDB, WrappedConnection),
            DBMS.MySQL => DatabaseHelperFactory.GetDatabaseHelper(DBMS.MySQL, WrappedConnection),
            DBMS.PostgreSQL => DatabaseHelperFactory.GetDatabaseHelper(DBMS.PostgreSQL, WrappedConnection),
            DBMS.SQLServer => DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLServer, WrappedConnection),
            _ => throw new NotSupportedException($"{typeof(T).Name} not supported.")
        };
        internal IEvolveMetadata MetadataTable => DbHelper.GetMetadataTable(SchemaName, "changelog");
        public string ScenarioFolder => Dbms switch
        {
            DBMS.Cassandra => Path.Combine(CassandraDb.SqlScriptsFolder, GetType().Name),
            DBMS.CockroachDB => Path.Combine(CockroachDB.SqlScriptsFolder, GetType().Name),
            DBMS.MySQL => Path.Combine(MySQL.SqlScriptsFolder, GetType().Name),
            DBMS.PostgreSQL => Path.Combine(PostgreSQL.SqlScriptsFolder, GetType().Name),
            DBMS.SQLServer => Path.Combine(SqlServer.SqlScriptsFolder, GetType().Name),
            _ => throw new NotSupportedException($"{typeof(T).Name} not supported.")
        };
    }
}
