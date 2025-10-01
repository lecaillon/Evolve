using EvolveDb.Configuration;
using EvolveDb.Dialect;
using EvolveDb.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EvolveDb.Tests.Dialect
{
    public class SqlLintIntegrationTest
    {
        private class TestSqlStatementBuilder : SqlStatementBuilderBase
        {
            public override string BatchDelimiter => ";";

            protected override IEnumerable<SqlStatement> Parse(string sqlScript, bool transactionEnabled)
            {
                if (string.IsNullOrWhiteSpace(sqlScript))
                    return new List<SqlStatement>();

                var statements = sqlScript.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var lineNumber = 1;

                return statements.Select(stmt => new SqlStatement(stmt.Trim(), transactionEnabled, lineNumber++));
            }
        }

        [Fact]
        public void LoadSqlStatements_WithLintingDisabled_DoesNotPerformLinting()
        {
            // Arrange
            var builder = new TestSqlStatementBuilder();
            var migration = new EmbeddedResourceMigrationScript("1.0", "test", "test.sql",
                new System.IO.MemoryStream("DROP TABLE users"u8.ToArray()),
                Metadata.MetadataType.Migration);
            var placeholders = new Dictionary<string, string>();

            // Act - using original method (no linting)
            var statements = builder.LoadSqlStatements(migration, placeholders);

            // Assert - should return statements without throwing
            Assert.Single(statements);
        }

        [Fact]
        public void LoadSqlStatements_WithLintingEnabledAndWarnings_LogsWarningsAndReturnsStatements()
        {
            // Arrange
            var builder = new TestSqlStatementBuilder();
            var migration = new EmbeddedResourceMigrationScript("1.0", "test", "test.sql",
                new System.IO.MemoryStream("DROP TABLE users"u8.ToArray()),
                Metadata.MetadataType.Migration);
            var placeholders = new Dictionary<string, string>();
            var logMessages = new List<string>();

            // Act
            var statements = builder.LoadSqlStatements(migration, placeholders,
                enableSqlLint: true,
                sqlLintFailureLevel: SqlLintFailureLevel.Warning,
                logAction: msg => logMessages.Add(msg));

            // Assert
            Assert.Single(statements);
            Assert.Single(logMessages);
            Assert.Contains("SQL Lint Warning", logMessages[0]);
            Assert.Contains("DROP TABLE", logMessages[0]);
        }

        [Fact]
        public void LoadSqlStatements_WithLintingEnabledAndErrors_ThrowsException()
        {
            // Arrange
            var builder = new TestSqlStatementBuilder();
            var migration = new EmbeddedResourceMigrationScript("1.0", "test", "test.sql",
                new System.IO.MemoryStream("DROP TABLE users;CREATE TABLE products (id INT)"u8.ToArray()),
                Metadata.MetadataType.Migration);
            var placeholders = new Dictionary<string, string>();

            // Act & Assert
            var exception = Assert.Throws<EvolveSqlLintException>(() =>
                builder.LoadSqlStatements(migration, placeholders,
                    enableSqlLint: true,
                    sqlLintFailureLevel: SqlLintFailureLevel.Error));

            Assert.Equal(2, exception.Issues.Count);
            Assert.Contains("DROP TABLE", exception.Message);
            Assert.Contains("CREATE TABLE", exception.Message);
        }

        [Fact]
        public void LoadSqlStatements_WithSafeSQLAndLintingEnabled_ReturnsStatementsWithoutIssues()
        {
            // Arrange
            var builder = new TestSqlStatementBuilder();
            var migration = new EmbeddedResourceMigrationScript("1.0", "test", "test.sql",
                new System.IO.MemoryStream("DROP TABLE IF EXISTS users;CREATE TABLE IF NOT EXISTS products (id INT)"u8.ToArray()),
                Metadata.MetadataType.Migration);
            var placeholders = new Dictionary<string, string>();
            var logMessages = new List<string>();

            // Act
            var statements = builder.LoadSqlStatements(migration, placeholders,
                enableSqlLint: true,
                sqlLintFailureLevel: SqlLintFailureLevel.Error,
                logAction: msg => logMessages.Add(msg));

            // Assert
            Assert.Equal(2, statements.Count());
            Assert.Empty(logMessages); // No warnings should be logged
        }

        [Fact]
        public void LoadSqlStatements_WithPlaceholdersAndLinting_ProcessesPlaceholdersBeforeLinting()
        {
            // Arrange
            var builder = new TestSqlStatementBuilder();
            var migration = new EmbeddedResourceMigrationScript("1.0", "test", "test.sql",
                new System.IO.MemoryStream("DROP TABLE ${table_name}"u8.ToArray()),
                Metadata.MetadataType.Migration);
            var placeholders = new Dictionary<string, string> { { "${table_name}", "users" } };
            var logMessages = new List<string>();

            // Act
            var statements = builder.LoadSqlStatements(migration, placeholders,
                enableSqlLint: true,
                sqlLintFailureLevel: SqlLintFailureLevel.Warning,
                logAction: msg => logMessages.Add(msg));

            // Assert
            var sqlStatements = statements as SqlStatement[] ?? statements.ToArray();
            Assert.Single(sqlStatements);
            Assert.Single(logMessages);
            Assert.Contains("DROP TABLE", logMessages[0]);

            // Verify placeholder was replaced
            var statement = sqlStatements.First();
            Assert.Contains("users", statement.Sql);
            Assert.DoesNotContain("${table_name}", statement.Sql);
        }

        [Fact]
        public void LoadSqlStatements_WithNullLintingParams_UsesDefaultBehavior()
        {
            // Arrange
            var builder = new TestSqlStatementBuilder();
            var migration = new EmbeddedResourceMigrationScript("1.0", "test", "test.sql",
                new System.IO.MemoryStream("DROP TABLE users"u8.ToArray()),
                Metadata.MetadataType.Migration);
            var placeholders = new Dictionary<string, string>();

            // Act - null linting params should disable linting
            var statements = builder.LoadSqlStatements(migration, placeholders,
                enableSqlLint: null,
                sqlLintFailureLevel: null);

            // Assert - should work without linting
            Assert.Single(statements);
        }
    }
}