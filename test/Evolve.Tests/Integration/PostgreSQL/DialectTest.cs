using EvolveDb.Connection;
using EvolveDb.Dialect;
using EvolveDb.Dialect.PostgreSQL;
using EvolveDb.Tests.Infrastructure;
using Xunit;

namespace EvolveDb.Tests.Integration.PostgregSql
{
    public class DialectTest : DbContainerFixture<PostgreSqlContainer>
    {
        public override bool MustRunContainer => TestContext.Local;

        [Fact]
        [Category(Test.PostgreSQL)]
        public void Run_all_PostgreSQL_integration_tests_work()
        {
            // Arrange
            var cnn = CreateDbConnection();
            var wcnn = new WrappedConnection(cnn).AssertDatabaseServerType(DBMS.PostgreSQL);
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.PostgreSQL, wcnn);
            string schemaName = "My metadata schema";
            var schema = new PostgreSQLSchema(schemaName, wcnn);

            // Assert
            schema.AssertIsNotExists();
            schema.AssertCreation();
            schema.AssertExists();
            schema.AssertIsEmpty();

            db.AssertDefaultSchemaName("public")
              .AssertApplicationLock(CreateDbConnection())
              .AssertMetadataTableCreation(schemaName, "changelog")
              .AssertMetadataTableLock()
              .AssertSchemaIsDroppableWhenNewSchemaFound(schemaName) // id:1
              .AssertVersionedMigrationSave() // id:2
              .AssertVersionedMigrationChecksumUpdate()
              .AssertRepeatableMigrationSave(); // id:3

            schema.AssertIsNotEmpty();
            schema.Erase();
            schema.AssertIsEmpty();
            schema.Drop();
            schema.AssertIsNotExists();

            db.AssertCloseConnection();
        }
    }
}
