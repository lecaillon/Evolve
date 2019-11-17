using System;
using System.Data;
using System.Linq;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.Cassandra;
using Evolve.Metadata;
using Xunit;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests
{
    internal static class AssertHelper
    {
        public static WrappedConnection AssertDatabaseServerType(this WrappedConnection wcnn, DBMS expectedDBMS)
        {
            Assert.Equal(expectedDBMS, wcnn.GetDatabaseServerType());
            return wcnn;
        }

        public static DatabaseHelper AssertDefaultSchemaName(this DatabaseHelper db, string expectedSchemaName)
        {
            string schemaName = db.GetCurrentSchemaName();
            Assert.True(schemaName == expectedSchemaName, $"The default schema should be '{expectedSchemaName}'.");
            return db;
        }

        public static IEvolveMetadata AssertMetadataTableCreation(this DatabaseHelper db, string schemaName, string tableName)
        {
            var metadataTable = db.GetMetadataTable(schemaName, tableName);
            Assert.False(metadataTable.IsExists(), "MetadataTable sould not already exist.");
            Assert.True(metadataTable.CreateIfNotExists(), "MetadataTable creation failed.");
            Assert.True(metadataTable.IsExists(), "MetadataTable should exist.");
            Assert.False(metadataTable.CreateIfNotExists(), "MetadataTable already exists. Creation should return false.");
            Assert.True(metadataTable.GetAllAppliedMigration().Count() == 0, "No migration metadata should be found.");
            Assert.True(metadataTable.GetAllAppliedRepeatableMigration().Count() == 0, "No repeatable migration metadata should be found.");
            return metadataTable;
        }

        public static IEvolveMetadata AssertMetadataTableLock(this IEvolveMetadata metadataTable)
        {
            Assert.True(metadataTable.TryLock());
            Assert.True(metadataTable.ReleaseLock());
            return metadataTable;
        }

        public static IEvolveMetadata AssertMetadataTableLockEx(this IEvolveMetadata metadataTable)
        {
            Assert.True(metadataTable.TryLock());
            Assert.False(metadataTable.TryLock());
            Assert.True(metadataTable.ReleaseLock());
            return metadataTable;
        }

        public static IEvolveMetadata AssertSchemaIsDroppableWhenNewSchemaFound(this IEvolveMetadata metadataTable, string schemaName)
        {
            metadataTable.Save(MetadataType.NewSchema, "0", "New schema created.", schemaName);
            Assert.True(metadataTable.CanDropSchema(schemaName), $"[{schemaName}] should be droppable.");
            Assert.False(metadataTable.CanEraseSchema(schemaName), $"[{schemaName}] should not be erasable.");
            return metadataTable;
        }

        public static IEvolveMetadata AssertSchemaIsErasableWhenEmptySchemaFound(this IEvolveMetadata metadataTable, string schemaName)
        {
            metadataTable.Save(MetadataType.EmptySchema, "0", "Empty schema found.", schemaName);
            Assert.False(metadataTable.CanDropSchema(schemaName), $"[{schemaName}] should not be droppable.");
            Assert.True(metadataTable.CanEraseSchema(schemaName), $"[{schemaName}] should be erasable.");
            return metadataTable;
        }

        public static IEvolveMetadata AssertVersionedMigrationSave(this IEvolveMetadata metadataTable)
        {
            metadataTable.SaveMigration(FileMigrationScriptV, true);
            Assert.True(metadataTable.GetAllAppliedMigration().Count() == 1, $"1 migration metadata should have been found, instead of {metadataTable.GetAllAppliedMigration().Count()}.");
            Assert.True(metadataTable.GetAllAppliedRepeatableMigration().Count() == 0, $"0 repeatable migration metadata should have been found, instead of {metadataTable.GetAllAppliedRepeatableMigration().Count()}.");
            var metadata = metadataTable.GetAllAppliedMigration().First();
            Assert.True(metadata.Version == FileMigrationScriptV.Version, $"Migration metadata version should be: 2.3.1, but found {metadata.Version}.");
            Assert.True(metadata.Checksum == FileMigrationScriptV.CalculateChecksum(), $"Migration metadata checksum should be: 6C7E36422F79696602E19079534B4076, but found {metadata.Checksum}.");
            Assert.True(metadata.Description == FileMigrationScriptV.Description, $"Migration metadata description should be: Duplicate migration script, but found {metadata.Description}.");
            Assert.True(metadata.Name == FileMigrationScriptV.Name, $"Migration metadata name should be: V2_3_1__Duplicate_migration_script.sql, but found {metadata.Name}.");
            Assert.True(metadata.Success == true, $"Migration metadata success should be: true, but found {metadata.Success}.");
            if (!(metadataTable is CassandraMetadataTable))
            {
                Assert.True(metadata.Id == 2, $"Migration metadata id should be: 2, but found {metadata.Id}.");
            }
            Assert.True(metadata.Type == MetadataType.Migration, $"Migration metadata type should be: Migration, but found {metadata.Type}.");
            Assert.True(metadata.InstalledOn.Date == DateTime.UtcNow.Date, $"Migration metadata InstalledOn date {metadata.InstalledOn.Date} should be equals to {DateTime.UtcNow.Date}.");
            return metadataTable;
        }

        public static IEvolveMetadata AssertVersionedMigrationChecksumUpdate(this IEvolveMetadata metadataTable, int metadataId = 2)
        {
            metadataTable.UpdateChecksum(metadataId, "Hi !");
            var metadata = metadataTable.GetAllAppliedMigration().Single(x => x.Id == metadataId);
            Assert.True(metadata.Checksum == "Hi !", $"Updated checksum should be: Hi!, but found {metadata.Checksum}");
            return metadataTable;
        }

        public static IEvolveMetadata AssertRepeatableMigrationSave(this IEvolveMetadata metadataTable)
        {
            metadataTable.SaveMigration(FileMigrationScriptR, true);
            Assert.True(metadataTable.GetAllAppliedMigration().Count() == 1, $"1 migration metadata should have been found, instead of {metadataTable.GetAllAppliedMigration().Count()}.");
            Assert.True(metadataTable.GetAllAppliedRepeatableMigration().Count() == 1, $"1 repeatable migration metadata should have been found, instead of {metadataTable.GetAllAppliedRepeatableMigration().Count()}.");
            var metadata = metadataTable.GetAllAppliedRepeatableMigration().First();
            Assert.True(metadata.Version == FileMigrationScriptR.Version, $"Repeatable migration metadata version should be: null, but found {metadata.Version}.");
            Assert.True(metadata.Checksum == FileMigrationScriptR.CalculateChecksum(), $"Repeatable migration metadata checksum should be; 71568061B2970A4B7C5160FE75356E10, but found {metadata.Checksum}.");
            Assert.True(metadata.Description == FileMigrationScriptR.Description, $"Repeatable migration metadata description should be: desc b, but found {metadata.Description}.");
            Assert.True(metadata.Name == FileMigrationScriptR.Name, $"Repeatable migration metadata name should be: R__desc_b.sql, but found {metadata.Name}.");
            Assert.True(metadata.Success == true, $"Repeatable migration metadata success should be: true, but found {metadata.Success}.");
            if (!(metadataTable is CassandraMetadataTable))
            {
                Assert.True(metadata.Id == 3, $"Repeatable migration metadata id should be: 3, but found {metadata.Id}.");
            }
            Assert.True(metadata.Type == MetadataType.RepeatableMigration, $"Repeatable migration metadata type should be: RepeatableMigration, but found {metadata.Type}.");
            Assert.True(metadata.InstalledOn.Date == DateTime.UtcNow.Date, $"Repeatable migration metadata InstalledOn date {metadata.InstalledOn.Date} should be equals to {DateTime.UtcNow.Date}.");
            return metadataTable;
        }

        public static Schema AssertIsNotExists(this Schema schema)
        {
            Assert.False(schema.IsExists(), $"The schema [{schema.Name}] should not already exist.");
            return schema;
        }

        public static Schema AssertExists(this Schema schema)
        {
            Assert.True(schema.IsExists(), $"The schema [{schema.Name}] should be created.");
            return schema;
        }

        public static Schema AssertCreation(this Schema schema)
        {
            Assert.True(schema.Create(), $"Creation of the schema [{schema.Name}] failed.");
            return schema;
        }

        public static Schema AssertIsNotEmpty(this Schema schema)
        {
            Assert.True(schema.IsExists(), $"The schema [{schema.Name}] should exist.");
            Assert.False(schema.IsEmpty(), $"[{schema.Name}] should not be empty.");
            return schema;
        }

        public static Schema AssertIsEmpty(this Schema schema)
        {
            Assert.True(schema.IsExists(), $"The schema [{schema.Name}] should exist.");
            Assert.True(schema.IsEmpty(), $"The schema [{schema.Name}] should be empty.");
            return schema;
        }

        public static DatabaseHelper AssertApplicationLock(this DatabaseHelper db, IDbConnection cnn2)
        {
            // Assert lock acquisition
            Assert.True(db.TryAcquireApplicationLock(), "Cannot acquire application lock.");

            // Can not acquire lock while it is taken by another connection
            var wcnn2 = new WrappedConnection(cnn2);
            var db2 = DatabaseHelperFactory.GetDatabaseHelper(wcnn2.GetDatabaseServerType(), wcnn2);
            Assert.False(db2.TryAcquireApplicationLock(), "Application lock should not have been acquired, because it is already handled.");

            // Assert lock is released
            Assert.True(db.ReleaseApplicationLock(), "Cannot release the application lock.");

            return db;
        }

        public static void AssertCloseConnection(this DatabaseHelper db)
        {
            db.Dispose();
            Assert.True(db.WrappedConnection.DbConnection.State == ConnectionState.Closed, "Database connection should be closed.");
        }

        public static Evolve AssertMigrateIsSuccessful(this Evolve evolve, IDbConnection cnn, int expectedNbMigration, Action<Evolve> arrange = null, params string[] locations)
        {
            if (locations.Any())
            {
                evolve.Locations = locations;
            }

            arrange?.Invoke(evolve);
            evolve.Migrate();
            Assert.True(evolve.NbMigration == expectedNbMigration, $"{expectedNbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no migration applied after a successful one, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            return evolve;
        }

        public static Evolve AssertMigrateThrows<T>(this Evolve evolve, IDbConnection cnn, Action<Evolve> arrange = null, params string[] locations) where T : Exception
        {
            if (locations.Any())
            {
                evolve.Locations = locations;
            }

            arrange?.Invoke(evolve);
            Assert.Throws<T>(() => evolve.Migrate());
            Assert.True(cnn.State == ConnectionState.Closed);

            return evolve;
        }

        public static Evolve AssertRepairIsSuccessful(this Evolve evolve, IDbConnection cnn, int expectedNbReparation, params string[] locations)
        {
            if (locations.Any())
            {
                evolve.Locations = locations;
            }

            evolve.Repair();
            Assert.True(evolve.NbReparation == expectedNbReparation, $"There should be {expectedNbReparation} migration repaired, not {evolve.NbReparation}.");
            Assert.True(cnn.State == ConnectionState.Closed);
            return evolve;
        }

        public static Evolve AssertEraseThrows<T>(this Evolve evolve, IDbConnection cnn, Action<Evolve> arrange) where T : Exception
        {
            arrange(evolve);
            Assert.Throws<T>(() => evolve.Erase());
            Assert.True(cnn.State == ConnectionState.Closed);

            return evolve;
        }

        public static Evolve AssertEraseIsSuccessful(this Evolve evolve, IDbConnection cnn, Action<Evolve> arrange = null)
        {
            arrange?.Invoke(evolve);
            evolve.Erase();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            return evolve;
        }

        public static Evolve AssertInfoIsSuccessful(this Evolve evolve, IDbConnection cnn, int expectedNbRows)
        {
            var rows = evolve.Info();
            int nbRows = rows?.Count() ?? 0;
            Assert.True(nbRows == expectedNbRows, $"{expectedNbRows} rows should have been returned, not {nbRows}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            return evolve;
        }
    }
}
