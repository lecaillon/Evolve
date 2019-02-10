﻿using System;
using System.Data;
using System.Data.SQLite;
using Evolve.Connection;
using Evolve.Tests.Infrastructure;
using Xunit;

namespace Evolve.Tests.Connection
{
    [Collection("PostgreSql collection")]
    public class WrappedConnectionTest
    {
        private static PostgreSqlFixture _pgContainer;

        public WrappedConnectionTest(PostgreSqlFixture pgContainer)
        {
            if (_pgContainer is null)
            {
                _pgContainer = pgContainer;

                if (TestContext.Local || TestContext.AzureDevOps)
                {
                    pgContainer.Run();
                }
            }
        }

        [Fact]
        public void Inner_dbconnection_can_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new WrappedConnection(null));
        }

        [Fact]
        public void When_disposed_inner_dbconnection_is_closed()
        {
            var cnn = _pgContainer.CreateDbConnection();
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                wrappedConnection.Open();
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact]
        public void When_opened_multiple_times_a_single_call_to_close_is_not_enought()
        {
            var cnn = _pgContainer.CreateDbConnection();
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                wrappedConnection.Open();
                wrappedConnection.Open();
                wrappedConnection.Open();

                wrappedConnection.Close();

                Assert.True(wrappedConnection.DbConnection.State == ConnectionState.Open);
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact]
        public void Commit_a_null_transaction_throws_InvalidOperationException()
        {
            var cnn = _pgContainer.CreateDbConnection();
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                Assert.Throws<InvalidOperationException>(() => wrappedConnection.Commit());
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact]
        public void Rollback_a_null_transaction_throws_InvalidOperationException()
        {
            var cnn = _pgContainer.CreateDbConnection();
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                Assert.Throws<InvalidOperationException>(() => wrappedConnection.Rollback());
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact]
        public void BeginTransaction_opens_a_connection_and_returns_a_transaction()
        {
            var cnn = _pgContainer.CreateDbConnection();
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                var tx = wrappedConnection.BeginTransaction();

                Assert.NotNull(wrappedConnection.CurrentTx);
                Assert.True(wrappedConnection.DbConnection.State == ConnectionState.Open);
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact]
        public void When_commit_transaction_is_cleared()
        {
            using (var wrappedConnection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var tx = wrappedConnection.BeginTransaction();
                wrappedConnection.Commit();

                Assert.Null(wrappedConnection.CurrentTx);
            }
        }

        [Fact]
        public void When_rollback_transaction_is_cleared()
        {
            using (var wrappedConnection = TestUtil.CreateSQLiteWrappedCnx())
            {
                var tx = wrappedConnection.BeginTransaction();
                wrappedConnection.Rollback();

                Assert.Null(wrappedConnection.CurrentTx);
            }
        }

        [Fact]
        public void When_dbconnection_is_ok_validation_works()
        {
            using (var wrappedConnection = TestUtil.CreateSQLiteWrappedCnx())
            {
                wrappedConnection.Validate();
            }
        }

        [Fact(Skip = "Skip test bescause it oddly fails on Linux")]
        public void When_dbconnection_is_not_ok_validation_fails()
        {
            using (var wrappedConnection = new WrappedConnection(new SQLiteConnection("Data Source=:fails")))
            {
                Assert.ThrowsAny<Exception>(() => wrappedConnection.Validate());
            }
        }

        [Fact]
        public void TryBeginTransaction_creates_a_tx_when_needed()
        {
            using (var cnx = TestUtil.CreateSQLiteWrappedCnx())
            {
                Assert.True(cnx.TryBeginTransaction());
                Assert.NotNull(cnx.CurrentTx);

                Assert.False(cnx.TryBeginTransaction());
                Assert.NotNull(cnx.CurrentTx);
            }
        }

        [Fact]
        public void TryCommit_returns_true_when_a_tx_exists()
        {
            using (var cnx = TestUtil.CreateSQLiteWrappedCnx())
            {
                Assert.False(cnx.TryCommit());
                Assert.Null(cnx.CurrentTx);

                cnx.BeginTransaction();
                Assert.True(cnx.TryCommit());
                Assert.Null(cnx.CurrentTx);
            }
        }

        [Fact]
        public void TryRollback_returns_true_when_a_tx_exists()
        {
            using (var cnx = TestUtil.CreateSQLiteWrappedCnx())
            {
                Assert.False(cnx.TryRollback());
                Assert.Null(cnx.CurrentTx);

                cnx.BeginTransaction();
                Assert.True(cnx.TryRollback());
                Assert.Null(cnx.CurrentTx);
            }
        }
    }
}
