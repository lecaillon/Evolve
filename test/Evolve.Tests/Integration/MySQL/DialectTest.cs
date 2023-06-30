using EvolveDb.Connection;
using EvolveDb.Dialect;
using EvolveDb.Dialect.MySQL;
using EvolveDb.Tests.Infrastructure;
using Xunit;

namespace EvolveDb.Tests.Integration.MySql
{
    public record DialectTest : DbContainerFixture<MySQLContainer>
    {
        [Fact]
        [Category(Test.MySQL)]
        public void Run_all_MySQL_integration_tests_work()
        {
            // Arrange
            var cnn = CreateDbConnection();
            var wcnn = new WrappedConnection(cnn).AssertDatabaseServerType(DBMS.MySQL);
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.MySQL, wcnn);
            string schemaName = "My metadata schema";
            var schema = new MySQLSchema(schemaName, wcnn);

            // Assert
            schema.Drop();
            schema.AssertIsNotExists();
            schema.AssertCreation();
            schema.AssertExists();
            schema.AssertIsEmpty();

            db.AssertDefaultSchemaName(MySQLContainer.DbName)
              .AssertApplicationLock(CreateDbConnection())
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
