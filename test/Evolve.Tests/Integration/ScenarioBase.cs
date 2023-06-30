using EvolveDb.Connection;
using EvolveDb.Dialect;
using EvolveDb.Metadata;
using EvolveDb.Tests.Infrastructure;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using Xunit.Abstractions;
using static EvolveDb.Tests.TestContext;

namespace EvolveDb.Tests.Integration
{
    public abstract class Scenario<T> : DbContainerFixture<T> where T : IDbContainer, new()
    {
        protected readonly T _dbContainer = new();
        protected readonly ITestOutputHelper _output;

        public Scenario(ITestOutputHelper output)
        {
            _output = output;
        }

        public DbConnection Cnn { get; private set; }
        internal WrappedConnection WrappedConnection { get; private set; }
        public Evolve Evolve { get; private set; }
        public DBMS Dbms => typeof(T).Name switch
        {
            "CassandraContainer" => DBMS.Cassandra,
            "CockroachDbContainer" => DBMS.CockroachDB,
            "MySQLContainer" => DBMS.MySQL,
            "PostgreSqlContainer" => DBMS.PostgreSQL,
            "SQLServerContainer" => DBMS.SQLServer,
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

        public override Action Initialize => Init;

        private void Init()
        {
            if (Dbms == DBMS.SQLServer)
            {
                TestUtil.CreateSqlServerDatabase(DbName, CnxStr);
                Cnn = new SqlConnection(CnxStr.Replace("master", DbName));
            }
            else
            {
                Cnn = _dbContainer.CreateDbConnection();
            }

            WrappedConnection = new WrappedConnection(Cnn);
            Evolve = new Evolve(Cnn, msg => _output.WriteLine(msg), Dbms)
            {
                Schemas = new[] { SchemaName },
                MetadataTableSchema = SchemaName,
                Locations = new[] { ScenarioFolder },
                Placeholders = new() { ["${db}"] = DbName, ["${schema}"] = SchemaName },
            };

            Evolve.Erase();
        }
    }
}
