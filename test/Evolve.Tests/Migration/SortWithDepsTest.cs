using EvolveDb.Metadata;
using EvolveDb.Migration;
using EvolveDb.Utilities;
using System.Linq;
using Xunit;

namespace EvolveDb.Tests.Migration
{
    public class SortWithDepsTest
    {
        [Fact]
        [Category(Test.Migration)]
        public void SortWithDeps_should_work()
        {
            // Arrange
            var config = new EvolveConfiguration
            {
                Locations = new[]
                {
                    TestContext.RepeatableWithDeps1,
                }
            };
            var loader = new FileMigrationLoader(config);

            // Act
            var scripts = loader
                .GetRepeatableMigrations()
                .SortWithDependencies(
                    config.SqlRepeatableMigrationPrefix,
                    config.SqlMigrationSeparator
                )
                .ToList();

            // Assert
            Assert.Equal(3, scripts.Count);
            AssertRepeatableMigration(scripts[0], "R__PROC_C.sql", "PROC C");
            AssertRepeatableMigration(scripts[1], "R__PROC_B.sql", "PROC B");
            AssertRepeatableMigration(scripts[2], "R__PROC_A.sql", "PROC A");

            static void AssertRepeatableMigration(MigrationScript migration, string name, string description)
            {
                Assert.Equal(MetadataType.RepeatableMigration, migration.Type);
                Assert.Equal(name, migration.Name);
                Assert.Equal(description, migration.Description);
            }
        }

        [Fact]
        [Category(Test.Migration)]
        public void SortWithDeps_should_fail_on_circular_dependency()
        {
            // Arrange
            var config = new EvolveConfiguration
            {
                Locations = new[]
                {
                    TestContext.RepeatableWithDeps2,
                }
            };
            var loader = new FileMigrationLoader(config);

            // Act/Assert
            Assert.Throws<EvolveException>(() =>
            {
                loader
                    .GetRepeatableMigrations()
                    .SortWithDependencies(
                        config.SqlRepeatableMigrationPrefix,
                        config.SqlMigrationSeparator
                    )
                    .ToList();
            });
        }
    }
}
