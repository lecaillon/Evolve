using EvolveDb.Dialect;
using System.Linq;
using Xunit;

namespace EvolveDb.Tests.Dialect
{
    public class SqlLinterTest
    {
        [Fact]
        public void AnalyzeStatements_WithSafeStatements_ReturnsNoIssues()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("CREATE TABLE IF NOT EXISTS users (id INT)", true),
                new SqlStatement("DROP TABLE IF EXISTS temp_table", true),
                new SqlStatement("CREATE SEQUENCE IF NOT EXISTS test_sequence", true),
                new SqlStatement("DROP SEQUENCE IF EXISTS old_sequence", true),
                new SqlStatement("INSERT INTO users VALUES (1)", true)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements);

            // Assert
            Assert.Empty(issues);
        }

        [Fact]
        public void AnalyzeStatements_WithDropTableWithoutIfExists_ReturnsIssue()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("DROP TABLE users", true, 5)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements).ToList();

            // Assert
            Assert.Single(issues);
            Assert.Equal(5, issues[0].LineNumber);
            Assert.Contains("DROP TABLE", issues[0].Message);
            Assert.Contains("IF EXISTS", issues[0].Message);
        }

        [Fact]
        public void AnalyzeStatements_WithCreateTableWithoutIfNotExists_ReturnsIssue()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("CREATE TABLE users (id INT)", true, 10)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements).ToList();

            // Assert
            Assert.Single(issues);
            Assert.Equal(10, issues[0].LineNumber);
            Assert.Contains("CREATE TABLE", issues[0].Message);
            Assert.Contains("IF NOT EXISTS", issues[0].Message);
        }

        [Fact]
        public void AnalyzeStatements_WithDropSchemaWithoutIfExists_ReturnsIssue()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("DROP SCHEMA test_schema", true, 15)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements).ToList();

            // Assert
            Assert.Single(issues);
            Assert.Equal(15, issues[0].LineNumber);
            Assert.Contains("DROP SCHEMA", issues[0].Message);
            Assert.Contains("IF EXISTS", issues[0].Message);
        }

        [Fact]
        public void AnalyzeStatements_WithDropDatabaseWithoutIfExists_ReturnsIssue()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("DROP DATABASE test_db", true, 20)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements).ToList();

            // Assert
            Assert.Single(issues);
            Assert.Equal(20, issues[0].LineNumber);
            Assert.Contains("DROP", issues[0].Message);
            Assert.Contains("DATABASE", issues[0].Message);
            Assert.Contains("IF EXISTS", issues[0].Message);
        }

        [Fact]
        public void AnalyzeStatements_WithCreateSchemaWithoutIfNotExists_ReturnsIssue()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("CREATE SCHEMA test_schema", true, 25)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements).ToList();

            // Assert
            Assert.Single(issues);
            Assert.Equal(25, issues[0].LineNumber);
            Assert.Contains("CREATE SCHEMA", issues[0].Message);
            Assert.Contains("IF NOT EXISTS", issues[0].Message);
        }

        [Fact]
        public void AnalyzeStatements_WithDropSequenceWithoutIfExists_ReturnsIssue()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("DROP SEQUENCE test_sequence", true, 30)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements).ToList();

            // Assert
            Assert.Single(issues);
            Assert.Equal(30, issues[0].LineNumber);
            Assert.Contains("DROP SEQUENCE", issues[0].Message);
            Assert.Contains("IF EXISTS", issues[0].Message);
        }

        [Fact]
        public void AnalyzeStatements_WithCreateSequenceWithoutIfNotExists_ReturnsIssue()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("CREATE SEQUENCE test_sequence", true, 35)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements).ToList();

            // Assert
            Assert.Single(issues);
            Assert.Equal(35, issues[0].LineNumber);
            Assert.Contains("CREATE SEQUENCE", issues[0].Message);
            Assert.Contains("IF NOT EXISTS", issues[0].Message);
        }

        [Fact]
        public void AnalyzeStatements_WithMultipleIssues_ReturnsAllIssues()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("DROP TABLE users", true, 1),
                new SqlStatement("CREATE TABLE products (id INT)", true, 2),
                new SqlStatement("DROP VIEW user_view", true, 3),
                new SqlStatement("DROP SEQUENCE test_sequence", true, 4),
                new SqlStatement("CREATE SEQUENCE new_sequence", true, 5)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements).ToList();

            // Assert
            Assert.Equal(5, issues.Count);
            Assert.Contains(issues, i => i.Message.Contains("DROP TABLE"));
            Assert.Contains(issues, i => i.Message.Contains("CREATE TABLE"));
            Assert.Contains(issues, i => i.Message.Contains("DROP VIEW"));
            Assert.Contains(issues, i => i.Message.Contains("DROP SEQUENCE"));
            Assert.Contains(issues, i => i.Message.Contains("CREATE SEQUENCE"));
        }

        [Fact]
        public void AnalyzeStatements_WithEmptyOrNullStatements_ReturnsNoIssues()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("", true),
                new SqlStatement("   ", true),
                new SqlStatement(null, true)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements);

            // Assert
            Assert.Empty(issues);
        }

        [Fact]
        public void AnalyzeStatements_WithCaseInsensitiveStatements_ReturnsIssues()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("drop table Users", true, 1),
                new SqlStatement("CREATE table Products (id int)", true, 2),
                new SqlStatement("Drop Schema TestSchema", true, 3)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements).ToList();

            // Assert
            Assert.Equal(3, issues.Count);
        }

        [Fact]
        public void AnalyzeStatements_WithCommentsAndComplexSQL_DoesNotCreateFalsePositives()
        {
            // Arrange
            var statements = new[]
            {
                new SqlStatement("-- This is a comment about DROP TABLE\nINSERT INTO users VALUES (1)", true),
                new SqlStatement("SELECT * FROM users WHERE name = 'CREATE TABLE test'", true),
                new SqlStatement("UPDATE logs SET message = 'DROP DATABASE occurred' WHERE id = 1", true)
            };

            // Act
            var issues = SqlLinter.AnalyzeStatements(statements);

            // Assert
            Assert.Empty(issues);
        }
    }
}