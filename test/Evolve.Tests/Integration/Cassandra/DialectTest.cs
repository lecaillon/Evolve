using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.Cassandra;
using Evolve.Tests.Infrastructure;
using Xunit;

namespace Evolve.Tests.Integration.Cassandra
{
    public class DialectTest
    {
        /// <summary>
        ///     Second part of the integration test.
        /// </summary>
        /// <remarks>
        ///     Due to some issues running 2 Cassandra containers, one after the other, 
        ///     in the same test context, we merge the integration tests to only use one container.
        ///     My guess, a possible Cassandra driver issue.
        /// </remarks>
        public static void Run_all_Cassandra_integration_tests_work(CassandraFixture dbContainer)
        {
            // Arrange
            var cnn = dbContainer.CreateDbConnection().AssertIsOpenned();
            var wcnn = new WrappedConnection(cnn).AssertDatabaseServerType(DBMS.Cassandra);
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.Cassandra, wcnn);
            string keyspaceName = "my_keyspace_2";
            var schema = new CassandraKeyspace(keyspaceName, CassandraKeyspace.CreateSimpleStrategy(1), wcnn);

            // Assert
            schema.AssertIsNotExists();
            schema.AssertCreation();
            schema.AssertExists();
            schema.AssertIsEmpty();

            //Lock & Unlock
            //..Applicaiton level: return true if the cluster lock keyspace/table is not present
            Assert.True(db.TryAcquireApplicationLock());
            wcnn.ExecuteNonQuery("create keyspace cluster_lock with replication = { 'class' : 'SimpleStrategy','replication_factor' : '1' }; ");
            Assert.True(db.TryAcquireApplicationLock()); //Still true, table is missing
            wcnn.ExecuteNonQuery("create table cluster_lock.lock (locked int, primary key(locked))");
            Assert.True(db.TryAcquireApplicationLock()); //Still true, lock is not present
            wcnn.ExecuteNonQuery("insert into cluster_lock.lock (locked) values (1) using TTL 3600");
            Assert.False(db.TryAcquireApplicationLock());
            wcnn.ExecuteNonQuery("drop keyspace cluster_lock");
            Assert.True(db.TryAcquireApplicationLock());
            Assert.True(db.ReleaseApplicationLock());
            Assert.True(db.ReleaseApplicationLock());

            db.AssertMetadataTableCreation(keyspaceName, "evolve_change_log")
              .AssertMetadataTableLock() //..Table level: lock implemented with LWT on evolve metadata table
              .AssertSchemaIsDroppableWhenNewSchemaFound(keyspaceName) // id:1
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
