using System.Collections.Generic;
using Evolve.Metadata;
using Evolve.Migration;
using Xunit;
using static Evolve.Tests.TestContext;
using static Evolve.Tests.TestUtil;

namespace Evolve.Tests.Migration
{
    public class MigrationBaseTest
    {
        [Fact]
        [Category(Test.Migration)]
        public void Migrations_should_be_well_sorted()
        {
            // Arrange
            var list = new List<MigrationBase>
            {
                BuildRepeatableEmbeddedResourceMigrationScript("a-name"),       // 9
                BuildMigrationMetadata("2"),                                    // 3
                BuildMigrationMetadata("1.1"),                                  // 1
                BuildMigrationMetadata("3.12.1"),                               // 8
                BuildRepeatableEmbeddedResourceMigrationScript("name"),         // 11
                BuildEmbeddedResourceMigrationScript("1"),                      // 0
                BuildEmbeddedResourceMigrationScript("1.1.0"),                  // 2
                BuildFileMigrationScript(SQLite.ChinookScriptPath, "2.1.0"),    // 4
                BuildFileMigrationScript(SQLite.ChinookScriptPath, "3.11.2"),   // 7
                BuildFileMigrationScript(SQLite.ChinookScriptPath, "2.1.1"),    // 5
                BuildFileMigrationScript(SQLite.ChinookScriptPath, "3.0"),      // 6
                BuildRepeatableFileMigrationScript(SQLite.ChinookScriptPath),   // 10
            };

            // Act
            list.Sort();

            // Assert
            Assert.Equal("1", list[0].Version.Label);
            Assert.Equal("1.1", list[1].Version.Label);
            Assert.Equal("1.1.0", list[2].Version.Label);
            Assert.Equal("2", list[3].Version.Label);
            Assert.Equal("2.1.0", list[4].Version.Label);
            Assert.Equal("2.1.1", list[5].Version.Label);
            Assert.Equal("3.0", list[6].Version.Label);
            Assert.Equal("3.11.2", list[7].Version.Label);
            Assert.Equal("3.12.1", list[8].Version.Label);
            Assert.Equal("a-name", list[9].Name);
            Assert.Equal("Chinook_Sqlite.sql", list[10].Name);
            Assert.Equal("name", list[11].Name);
        }

        [Fact]
        [Category(Test.Migration)]
        public void Migration_comparaison_should_be_logical()
        {
            Assert.True(BuildFileMigrationScript(SQLite.ChinookScriptPath, "1") == BuildEmbeddedResourceMigrationScript("1"));
            Assert.True(BuildRepeatableFileMigrationScript(SQLite.ChinookScriptPath) == BuildRepeatableFileMigrationScript(SQLite.ChinookScriptPath, "desc1", "1.0"));

            Assert.True(BuildFileMigrationScript(SQLite.ChinookScriptPath, "1") != BuildFileMigrationScript(SQLite.ChinookScriptPath, "1.0"));
            Assert.True(BuildRepeatableEmbeddedResourceMigrationScript("a-name") != BuildRepeatableEmbeddedResourceMigrationScript("b-name"));
            Assert.True(new MigrationMetadata(version: null, "desc", "a-name", MetadataType.RepeatableMigration) != new MigrationMetadata("1", "desc", "a-name", MetadataType.Migration));

            Assert.True(BuildMigrationMetadata("1") < BuildMigrationMetadata("2"));
            Assert.True(BuildRepeatableEmbeddedResourceMigrationScript("a-name") < BuildRepeatableEmbeddedResourceMigrationScript("b-name"));
            Assert.True(BuildFileMigrationScript(SQLite.ChinookScriptPath, "1.1.1") < BuildFileMigrationScript(SQLite.ChinookScriptPath, "1.1.2.0"));
            Assert.True(BuildFileMigrationScript(SQLite.ChinookScriptPath, "2") < BuildRepeatableEmbeddedResourceMigrationScript("b-name"));

            Assert.True(BuildMigrationMetadata("2") > BuildMigrationMetadata("1"));
            Assert.True(BuildRepeatableEmbeddedResourceMigrationScript("b-name") > BuildRepeatableEmbeddedResourceMigrationScript("a-name"));
            Assert.True(BuildRepeatableEmbeddedResourceMigrationScript("a-name") > BuildFileMigrationScript(SQLite.ChinookScriptPath, "0.1"));
            Assert.True(BuildMigrationMetadata("1") > BuildFileMigrationScript(SQLite.ChinookScriptPath, "0.5.1"));
        }
    }
}
