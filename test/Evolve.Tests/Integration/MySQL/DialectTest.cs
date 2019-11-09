using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.MySQL;
using Evolve.Tests.Infrastructure;
using Xunit;

namespace Evolve.Tests.Integration.MySql
{
    [Collection("MySQL collection")]
    public class DialectTest
    {
        private readonly MySQLFixture _dbContainer;

        public DialectTest(MySQLFixture dbContainer)
        {
            _dbContainer = dbContainer;

            if (TestContext.Local)
            {
                dbContainer.Run(fromScratch: true);
            }
        }

        [Fact]
        [Category(Test.MySQL)]
        public void Run_all_MySQL_integration_tests_work()
        {
            // Arrange
            var cnn = _dbContainer.CreateDbConnection();
            var wcnn = new WrappedConnection(cnn).AssertDatabaseServerType(DBMS.MySQL);
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.MySQL, wcnn);
            string schemaName = "My metadata schema";
            var schema = new MySQLSchema(schemaName, wcnn);

            // Assert
            schema.AssertIsNotExists();
            schema.AssertCreation();
            schema.AssertExists();
            schema.AssertIsEmpty();

            db.AssertDefaultSchemaName(MySQLContainer.DbName)
              .AssertApplicationLock(_dbContainer.CreateDbConnection())
              .AssertMetadataTableCreation(schemaName, "change log")
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
