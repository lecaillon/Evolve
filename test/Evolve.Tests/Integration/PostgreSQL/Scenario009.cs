using EvolveDb.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace EvolveDb.Tests.Integration.PostgreSql
{
    [Collection("PostgreSql collection")]
    public class Scenario009 : Scenario<PostgreSqlFixture>
    {
        public Scenario009(PostgreSqlFixture dbContainer, ITestOutputHelper output) : base(dbContainer, output) {}

        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Scenario_drop_extension_should_work()
        {
            Evolve.Migrate();

            // Do not use Evolve.Erase() because in this case it will execute DROP SCHEMA
            // We want to force erase of the schema object by object.
            DbHelper.GetSchema(SchemaName).Erase();

            Assert.True(DbHelper.GetSchema(SchemaName).IsExists(), $"The schema [{SchemaName}] should exist.");
            Assert.True(DbHelper.GetSchema(SchemaName).IsEmpty(), $"The schema [{SchemaName}] should be empty.");
        }
    }
}
