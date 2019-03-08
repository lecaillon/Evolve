using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.PostgreSQL;
using Evolve.Tests.Infrastructure;
using Xunit;

namespace Evolve.Tests.Integration.PostgreSQL
{
    [Collection("PostgreSql collection")]
    public class DialectTest
    {
        private readonly PostgreSqlFixture _dbContainer;

        public DialectTest(PostgreSqlFixture pgContainer)
        {
            _dbContainer = pgContainer;

            if (TestContext.Local)
            {
                pgContainer.Run(fromScratch: true);
            }
        }

        [Fact]
        [Category(Test.PostgreSQL)]
        public void Run_all_PostgreSQL_integration_tests_work()
        {
            // Arrange
            var cnn = _dbContainer.CreateDbConnection().AssertIsOpenned();
            var wcnn = new WrappedConnection(cnn).AssertDatabaseServerType(DBMS.PostgreSQL);
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.PostgreSQL, wcnn);
            string schemaName = "My metadata schema";
            var schema = new PostgreSQLSchema(schemaName, wcnn);

            // Assert
            var metadataTable = db.AssertDefaultSchemaName("public")
                                  .AssertApplicationLock(_dbContainer.CreateDbConnection())
                                  .AssertMetadataTableCreation(schemaName, "changelog")
                                  .AssertMetadataTableLock();

            schema.AssertIsNotExists();
            schema.AssertCreation();
            schema.AssertExists();
            schema.AssertIsEmpty();

            metadataTable.AssertSchemaIsDroppableWhenNewSchemaFound(schemaName) // id:1
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
