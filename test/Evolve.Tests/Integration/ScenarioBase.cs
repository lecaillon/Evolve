using System;
using System.Data.Common;
using System.IO;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Metadata;
using Evolve.Tests.Infrastructure;
using Xunit.Abstractions;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.PostgreSql
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

            Cnn = _dbContainer.CreateDbConnection();
            WrappedConnection = new WrappedConnection(Cnn);
            Evolve = new Evolve(Cnn, msg => _output.WriteLine(msg))
            {

                Schemas = new[] { SchemaName },
                MetadataTableSchema = SchemaName,
                Locations = new[] { ScenarioFolder },
                Placeholders = new() { ["${schema}"] = SchemaName },
            };
        }

        public DbConnection Cnn { get; }
        internal WrappedConnection WrappedConnection { get; }
        public Evolve Evolve { get; }
        internal DatabaseHelper DbHelper => typeof(T).Name switch
        {
            "CassandraFixture" => DatabaseHelperFactory.GetDatabaseHelper(DBMS.Cassandra, WrappedConnection),
            "CockroachDbFixture" => DatabaseHelperFactory.GetDatabaseHelper(DBMS.CockroachDB, WrappedConnection),
            "MySQLFixture" => DatabaseHelperFactory.GetDatabaseHelper(DBMS.MySQL, WrappedConnection),
            "PostgreSqlFixture" => DatabaseHelperFactory.GetDatabaseHelper(DBMS.PostgreSQL, WrappedConnection),
            "SQLServerFixture" => DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLServer, WrappedConnection),
            _ => throw new NotSupportedException($"{typeof(T).Name} not supported.")
        };
        internal IEvolveMetadata MetadataTable => DbHelper.GetMetadataTable(SchemaName, "changelog");
        public string SchemaName => GetType().Name.ToLower();
        public string ScenarioFolder => typeof(T).Name switch
        {
            "CassandraFixture" => Path.Combine(CassandraDb.SqlScriptsFolder, GetType().Name),
            "CockroachDbFixture" => Path.Combine(CockroachDB.SqlScriptsFolder, GetType().Name),
            "MySQLFixture" => Path.Combine(MySQL.SqlScriptsFolder, GetType().Name),
            "PostgreSqlFixture" => Path.Combine(PostgreSQL.SqlScriptsFolder, GetType().Name),
            "SQLServerFixture" => Path.Combine(SqlServer.SqlScriptsFolder, GetType().Name),
            _ => throw new NotSupportedException($"{typeof(T).Name} not supported.")
        };
    }
}
