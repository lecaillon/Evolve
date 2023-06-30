﻿using EvolveDb.Connection;
using EvolveDb.Dialect;
using EvolveDb.Dialect.CockroachDB;
using EvolveDb.Tests.Infrastructure;

namespace EvolveDb.Tests.Integration.CockroachDb
{
    public record DialectTest : DbContainerFixture<CockroachDBContainer>
    {
        [FactSkippedOnAppVeyor]
        [Category(Test.CockroachDB)]
        public void Run_all_CockroachDB_integration_tests_work()
        {
            // Arrange
            var cnn = CreateDbConnection();
            var wcnn = new WrappedConnection(cnn).AssertDatabaseServerType(DBMS.CockroachDB);
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.CockroachDB, wcnn);
            string schemaName = "MyDatabase";
            Schema schema = new CockroachDBDatabase(schemaName, wcnn);

            // Assert
            schema.AssertIsNotExists();
            schema.AssertCreation();
            schema.AssertExists();
            schema.AssertIsEmpty();

            db.AssertDefaultSchemaName("defaultdb")
              .AssertMetadataTableCreation(schemaName, "changelog")
              .AssertMetadataTableLockEx()
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
