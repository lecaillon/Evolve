using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.CockroachDB;
using Evolve.Tests.Infrastructure;
using Xunit;

namespace Evolve.Tests.Integration.CockroachDb
{
    [Collection("CockroachDB collection")]
    public class DialectTest
    {
        private readonly CockroachDBFixture _dbContainer;

        public DialectTest(CockroachDBFixture dbContainer)
        {
            _dbContainer = dbContainer;

            if (TestContext.Local)
            {
                dbContainer.Run(fromScratch: true);
            }
        }

        [FactSkippedOnAppVeyor]
        [Category(Test.CockroachDB)]
        public void Run_all_CockroachDB_integration_tests_work()
        {
            // Arrange
            var cnn = _dbContainer.CreateDbConnection().AssertIsOpenned();
            var wcnn = new WrappedConnection(cnn).AssertDatabaseServerType(DBMS.CockroachDB);
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.CockroachDB, wcnn);
            string schemaName = "MyDatabase";
            Schema database = new CockroachDBDatabase(schemaName, wcnn);

            // Assert
            database.AssertIsNotExists();
            database.AssertCreation();
            database.AssertExists();
            database.AssertIsEmpty();

            db.AssertDefaultSchemaName("defaultdb")
              .AssertMetadataTableCreation(schemaName, "changelog")
              .AssertMetadataTableLockEx()
              .AssertSchemaIsDroppableWhenNewSchemaFound(schemaName) // id:1
              .AssertVersionedMigrationSave() // id:2
              .AssertVersionedMigrationChecksumUpdate()
              .AssertRepeatableMigrationSave(); // id:3

            database.AssertIsNotEmpty();
            database.Erase();
            database.AssertIsEmpty();
            database.Drop();
            database.AssertIsNotExists();

            db.AssertCloseConnection();
        }
    }
}
