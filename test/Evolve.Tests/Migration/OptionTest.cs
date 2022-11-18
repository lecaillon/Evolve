using EvolveDb.Metadata;
using EvolveDb.Migration;
using EvolveDb.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EvolveDb.Tests.Migration
{
    public class TestRepeatableMigrationScript : MigrationScript
    {
        public TestRepeatableMigrationScript(
            string description,
            string name,
            string content
        ) : base("", description, name, content, MetadataType.RepeatableMigration)
        {
        }
    }

    public class OptionTest
    {
        private MigrationScript CreateMigrationScript(params string[] lines)
        {
            return new TestRepeatableMigrationScript(
                "Test",
                "R__Test.sql",
                string.Join("\n", lines)
            );
        }

        [Fact]
        [Category(Test.Migration)]
        public void Can_parse_multiple_line_file_options()
        {
            var script = CreateMigrationScript(
                "-- evolve-tx-off",
                "-- evolve-evolve-repeat-always",
                "-- evolve-repeatable-deps=A|B|C"
            );

            Assert.True(script.MustRepeatAlways);
            Assert.False(script.IsTransactionEnabled);
            var deps = script.RepeatableDependencies.ToList();
            Assert.Equal(3, deps.Count);
            Assert.Equal("A", deps[0]);
            Assert.Equal("B", deps[1]);
            Assert.Equal("C", deps[2]);
        }

        [Fact]
        [Category(Test.Migration)]
        public void Can_parse_single_line_file_options()
        {
            var script = CreateMigrationScript(
                "-- evolve-tx-off evolve-evolve-repeat-always evolve-repeatable-deps = A | B | C"
            );

            Assert.True(script.MustRepeatAlways);
            Assert.False(script.IsTransactionEnabled);
            var deps = script.RepeatableDependencies.ToList();
            Assert.Equal(3, deps.Count);
            Assert.Equal("A", deps[0]);
            Assert.Equal("B", deps[1]);
            Assert.Equal("C", deps[2]);
        }

        [Fact]
        [Category(Test.Migration)]
        public void Ignore_file_options()
        {
            var script = CreateMigrationScript(
                "GO",
                "-- evolve-tx-off evolve-evolve-repeat-always evolve-repeatable-deps = A | B | C"
            );

            Assert.False(script.MustRepeatAlways);
            Assert.True(script.IsTransactionEnabled);
            var deps = script.RepeatableDependencies.ToList();
            Assert.Empty(deps);
        }

    }
}
