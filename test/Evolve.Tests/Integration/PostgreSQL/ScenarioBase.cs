using System.Data.Common;
using System.IO;
using Evolve.Tests.Infrastructure;
using Xunit.Abstractions;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.PostgreSql
{
    public abstract class ScenarioBase
    {
        protected readonly PostgreSqlFixture _dbContainer;
        protected readonly ITestOutputHelper _output;

        public ScenarioBase(PostgreSqlFixture dbContainer, ITestOutputHelper output)
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
        public string ScenarioFolder => Path.Combine(PostgreSQL.SqlScriptsFolder, GetType().Name);
    }
}
