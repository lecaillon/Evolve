using System;
using System.IO;
using System.Text;
using Evolve.Metadata;
using Evolve.Migration;
using Xunit;

namespace Evolve.Tests.Migration
{
    public class EmbeddedResourceMigrationScriptTest
    {
        [Fact]
        [Category(Test.Migration)]
        public void Should_have_a_valid_embedded_resource_migration()
        {
            var migration = new EmbeddedResourceMigrationScript(description: "desc",
                                                                name: "name",
                                                                content: new MemoryStream(Encoding.UTF8.GetBytes("content")));

            Assert.Null(migration.Version);
            Assert.Equal("content", migration.Content);
            Assert.Equal(MetadataType.RepeatableMigration, migration.Type);
        }
    }
}
