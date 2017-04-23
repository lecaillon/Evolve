using System;
using System.Data;
using Evolve.Connection;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Evolve.Core.Test.Connection
{
    public class WrappedConnectionTest
    {
        [Fact(DisplayName = "Inner_dbconnection_can_not_be_null")]
        public void Inner_dbconnection_can_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new WrappedConnection(null));
        }

        [Fact(DisplayName = "When_disposed_inner_dbconnection_is_closed")]
        public void When_disposed_inner_dbconnection_is_closed()
        {
            var cnn = new SqliteConnection("Data Source=:memory:");
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                wrappedConnection.Open();
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact(DisplayName = "When_opened_multiple_times_a_single_call_to_close_is_not_enought")]
        public void When_opened_multiple_times_a_single_call_to_close_is_not_enought()
        {
            var cnn = new SqliteConnection("Data Source=:memory:");
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

        [Fact(DisplayName = "Commit_a_null_transaction_throws_InvalidOperationException")]
        public void Commit_a_null_transaction_throws_InvalidOperationException()
        {
            var cnn = new SqliteConnection("Data Source=:memory:");
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                Assert.Throws<InvalidOperationException>(() => wrappedConnection.Commit());
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact(DisplayName = "Rollback_a_null_transaction_throws_InvalidOperationException")]
        public void Rollback_a_null_transaction_throws_InvalidOperationException()
        {
            var cnn = new SqliteConnection("Data Source=:memory:");
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                Assert.Throws<InvalidOperationException>(() => wrappedConnection.Rollback());
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact(DisplayName = "BeginTransaction_opens_a_connection_and_returns_a_transaction")]
        public void BeginTransaction_opens_a_connection_and_returns_a_transaction()
        {
            var cnn = new SqliteConnection("Data Source=:memory:");
            using (var wrappedConnection = new WrappedConnection(cnn))
            {
                var tx = wrappedConnection.BeginTransaction();

                Assert.NotNull(wrappedConnection.CurrentTx);
                Assert.True(wrappedConnection.DbConnection.State == ConnectionState.Open);
            }

            Assert.True(cnn.State == ConnectionState.Closed);
        }

        [Fact(DisplayName = "After_commit_transaction_is_cleared")]
        public void When_commit_transaction_is_cleared()
        {
            using (var wrappedConnection = new WrappedConnection(new SqliteConnection("Data Source=:memory:")))
            {
                var tx = wrappedConnection.BeginTransaction();
                wrappedConnection.Commit();

                Assert.Null(wrappedConnection.CurrentTx);
            }
        }

        [Fact(DisplayName = "When_rollback_transaction_is_cleared")]
        public void When_rollback_transaction_is_cleared()
        {
            using (var wrappedConnection = new WrappedConnection(new SqliteConnection("Data Source=:memory:")))
            {
                var tx = wrappedConnection.BeginTransaction();
                wrappedConnection.Rollback();

                Assert.Null(wrappedConnection.CurrentTx);
            }
        }

        [Fact(DisplayName = "When_dbconnection_is_ok_validation_works")]
        public void When_dbconnection_is_ok_validation_works()
        {
            using (var wrappedConnection = new WrappedConnection(new SqliteConnection("Data Source=:memory:")))
            {
                wrappedConnection.Validate();
            }
        }

        [Fact(DisplayName = "When_dbconnection_is_not_ok_validation_fails", Skip = "Skip test bescause it oddly fails on Linux")]
        public void When_dbconnection_is_not_ok_validation_fails()
        {
            using (var wrappedConnection = new WrappedConnection(new SqliteConnection("Data Source=:fails")))
            {
                Assert.ThrowsAny<Exception>(() => wrappedConnection.Validate());
            }
        }
    }
}
