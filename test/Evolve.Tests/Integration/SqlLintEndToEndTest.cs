using EvolveDb.Configuration;
using EvolveDb.Migration;
using System;
using System.Collections.Generic;
using System.IO; // Added for temp directory & file creation
using System.Linq;
using Xunit;

namespace EvolveDb.Tests.Integration
{
    public class SqlLintEndToEndTest
    {
        [Fact]
        public void SqlLint_WithEnabledWarnings_LogsWarningsAndContinues()
        {
            // Arrange
            var tempDir = CreateUnsafeMigrationDirectory();
            try
            {
                var migrationLoader = new FileMigrationLoader(new EvolveConfiguration
                {
                    Locations = [tempDir],
                    SqlMigrationPrefix = "V",
                    SqlMigrationSeparator = "__",
                    SqlMigrationSuffix = ".sql"
                });

                var migrations = migrationLoader.GetMigrations().ToList();
                var unsafeMigration = migrations.FirstOrDefault(m => m.Name.Contains("unsafe", StringComparison.OrdinalIgnoreCase));
                Assert.NotNull(unsafeMigration);

                var builder = new TestSqlStatementBuilder();
                var placeholders = new Dictionary<string, string>();
                var logMessages = new List<string>();

                // Act - linting enabled with warnings
                var statements = builder.LoadSqlStatements(unsafeMigration!, placeholders,
                    enableSqlLint: true,
                    sqlLintFailureLevel: SqlLintFailureLevel.Warning,
                    logAction: msg => logMessages.Add(msg));

                // Assert - statements still processed and warnings logged
                Assert.Equal(2, statements.Count()); // 2 statements in unsafe file
                Assert.Equal(2, logMessages.Count); // 2 lint issues => 2 warnings
                Assert.Contains(logMessages, m => m.Contains("DROP TABLE", StringComparison.OrdinalIgnoreCase));
                Assert.Contains(logMessages, m => m.Contains("CREATE TABLE", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, recursive: true); } catch { /* ignore */ }
                }
            }
        }

        [Fact]
        public void SqlLint_WithEnabledErrors_ThrowsOnUnsafeMigration()
        {
            // Arrange
            var tempDir = CreateUnsafeMigrationDirectory();
            try
            {
                var migrationLoader = new FileMigrationLoader(new EvolveConfiguration
                {
                    Locations = [tempDir],
                    SqlMigrationPrefix = "V",
                    SqlMigrationSeparator = "__",
                    SqlMigrationSuffix = ".sql"
                });

                var migrations = migrationLoader.GetMigrations().ToList();
                var unsafeMigration = migrations.FirstOrDefault(m => m.Name.Contains("unsafe", StringComparison.OrdinalIgnoreCase));
                Assert.NotNull(unsafeMigration);

                var builder = new TestSqlStatementBuilder();
                var placeholders = new Dictionary<string, string>();

                // Act & Assert
                var exception = Assert.Throws<EvolveSqlLintException>(() =>
                    builder.LoadSqlStatements(unsafeMigration!, placeholders,
                        enableSqlLint: true,
                        sqlLintFailureLevel: SqlLintFailureLevel.Error));

                Assert.Equal(2, exception.Issues.Count);
                Assert.Contains("DROP TABLE", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("CREATE TABLE", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, recursive: true); } catch { /* ignore */ }
                }
            }
        }

        [Fact]
        public void SqlLint_WithDisabled_ProcessesAllMigrationsWithoutLinting()
        {
            // Arrange
            var tempDir = CreateUnsafeMigrationDirectory();
            try
            {
                var migrationLoader = new FileMigrationLoader(new EvolveConfiguration
                {
                    Locations = [tempDir],
                    SqlMigrationPrefix = "V",
                    SqlMigrationSeparator = "__",
                    SqlMigrationSuffix = ".sql"
                });

                var migrations = migrationLoader.GetMigrations().ToList();
                var unsafeMigration = migrations.FirstOrDefault(m => m.Name.Contains("unsafe", StringComparison.OrdinalIgnoreCase));
                Assert.NotNull(unsafeMigration);

                var builder = new TestSqlStatementBuilder();
                var placeholders = new Dictionary<string, string>();

                // Act - Linting disabled
                var statements = builder.LoadSqlStatements(unsafeMigration!, placeholders,
                    enableSqlLint: false,
                    sqlLintFailureLevel: SqlLintFailureLevel.Error);

                // Assert - Should process without errors even with unsafe SQL
                Assert.Equal(2, statements.Count());
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, recursive: true); } catch { /* ignore */ }
                }
            }
        }

        private static string CreateUnsafeMigrationDirectory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "evolve_sql_lint_tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var migrationPath = Path.Combine(tempDir, "V1__unsafe.sql");
            // Two unsafe statements: missing IF EXISTS / IF NOT EXISTS protections
            File.WriteAllText(migrationPath, "DROP TABLE users;CREATE TABLE users (id INT);");
            return tempDir;
        }

        private class TestSqlStatementBuilder : EvolveDb.Dialect.SqlStatementBuilderBase
        {
            public override string BatchDelimiter => ";";

            protected override IEnumerable<EvolveDb.Dialect.SqlStatement> Parse(string sqlScript, bool transactionEnabled)
            {
                if (string.IsNullOrWhiteSpace(sqlScript))
                    return new List<EvolveDb.Dialect.SqlStatement>();

                var statements = sqlScript.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var lineNumber = 1;

                return statements.Select(stmt => new EvolveDb.Dialect.SqlStatement(stmt.Trim(), transactionEnabled, lineNumber++));
            }
        }
    }
}