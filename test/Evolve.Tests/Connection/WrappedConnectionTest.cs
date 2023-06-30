using System;
using System.Data;
using System.Data.SQLite;
using EvolveDb.Connection;
using EvolveDb.Tests.Infrastructure;
using Xunit;

namespace EvolveDb.Tests.Connection
{
    public record WrappedConnectionTest : DbContainerFixture<PostgreSqlContainer>
    {
        [Fact]
        [Category(Test.Connection)]
        public void Inner_dbconnection_can_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new WrappedConnection(null));
        }

        [Fact]
        [Category(Test.Connection)]
        public void When_disposed_inner_dbconnection_is_closed()
        {
            var cnn = CreateDbConnection();
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                wrappedConnection.Open();
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact]
        [Category(Test.Connection)]
        public void Commit_a_null_transaction_throws_InvalidOperationException()
        {
            var cnn = CreateDbConnection();
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                Assert.Throws<InvalidOperationException>(() => wrappedConnection.Commit());
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact]
        [Category(Test.Connection)]
        public void Rollback_a_null_transaction_throws_InvalidOperationException()
        {
            var cnn = CreateDbConnection();
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                Assert.Throws<InvalidOperationException>(() => wrappedConnection.Rollback());
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact]
        [Category(Test.Connection)]
        public void BeginTransaction_opens_a_connection_and_returns_a_transaction()
        {
            var cnn = CreateDbConnection();
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                var tx = wrappedConnection.BeginTransaction();

                Assert.NotNull(wrappedConnection.CurrentTx);
                Assert.True(wrappedConnection.DbConnection.State == ConnectionState.Open);
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact]
        [Category(Test.Connection)]
        public void When_commit_transaction_is_cleared()
        {
            using var wrappedConnection = TestUtil.CreateSQLiteWrappedCnx();
            var tx = wrappedConnection.BeginTransaction();
            wrappedConnection.Commit();

            Assert.Null(wrappedConnection.CurrentTx);
        }

        [Fact]
        [Category(Test.Connection)]
        public void When_rollback_transaction_is_cleared()
        {
            using var wrappedConnection = TestUtil.CreateSQLiteWrappedCnx();
            var tx = wrappedConnection.BeginTransaction();
            wrappedConnection.Rollback();

            Assert.Null(wrappedConnection.CurrentTx);
        }

        [Fact]
        [Category(Test.Connection)]
        public void When_dbconnection_is_ok_validation_works()
        {
            using var wrappedConnection = TestUtil.CreateSQLiteWrappedCnx();
            wrappedConnection.Validate();
        }

        [Fact(Skip = "Skip test bescause it oddly fails on Linux")]
        [Category(Test.Connection)]
        public void When_dbconnection_is_not_ok_validation_fails()
        {
            using var wrappedConnection = new WrappedConnection(new SQLiteConnection("Data Source=:fails"));
            Assert.ThrowsAny<Exception>(() => wrappedConnection.Validate());
        }

        [Fact]
        [Category(Test.Connection)]
        public void TryBeginTransaction_creates_a_tx_when_needed()
        {
            using var cnx = TestUtil.CreateSQLiteWrappedCnx();
            Assert.True(cnx.TryBeginTransaction());
            Assert.NotNull(cnx.CurrentTx);

            Assert.False(cnx.TryBeginTransaction());
            Assert.NotNull(cnx.CurrentTx);
        }

        [Fact]
        [Category(Test.Connection)]
        public void TryCommit_returns_true_when_a_tx_exists()
        {
            using var cnx = TestUtil.CreateSQLiteWrappedCnx();
            Assert.False(cnx.TryCommit());
            Assert.Null(cnx.CurrentTx);

            cnx.BeginTransaction();
            Assert.True(cnx.TryCommit());
            Assert.Null(cnx.CurrentTx);
        }

        [Fact]
        [Category(Test.Connection)]
        public void TryRollback_returns_true_when_a_tx_exists()
        {
            using var cnx = TestUtil.CreateSQLiteWrappedCnx();
            Assert.False(cnx.TryRollback());
            Assert.Null(cnx.CurrentTx);

            cnx.BeginTransaction();
            Assert.True(cnx.TryRollback());
            Assert.Null(cnx.CurrentTx);
        }
    }
}
