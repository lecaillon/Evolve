using System;
using System.Data.Common;
using System.IO;
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
            Evolve = new Evolve(Cnn, msg => _output.WriteLine(msg))
            {

                Schemas = new[] { ScenarioName },
                MetadataTableSchema = ScenarioName,
                Locations = new[] { ScenarioFolder },
                Placeholders = new() { ["${schema}"] = ScenarioName },
            };
        }

        protected DbConnection Cnn { get; }
        protected Evolve Evolve { get; }
        public string ScenarioName => GetType().Name.ToLower();
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
