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
            var migration = new EmbeddedResourceMigrationScript(version: "useless",
                                                                description: "desc",
                                                                name: "name",
                                                                content: new MemoryStream(Encoding.UTF8.GetBytes("content")),
                                                                MetadataType.RepeatableMigration);

            Assert.Null(migration.Version);
            Assert.Equal("content", migration.Content);
            Assert.Equal(MetadataType.RepeatableMigration, migration.Type);
        }

        [Fact]
        [Category(Test.Migration)]
        public void Should_throw_NotSupportedException_when_type_is_not_migration_or_repeatable_migration()
        {
            Assert.Throws<NotSupportedException>(() => new EmbeddedResourceMigrationScript(version: null, 
                                                                                           description: "desc", 
                                                                                           name: "name", 
                                                                                           content: new MemoryStream(Encoding.UTF8.GetBytes("content")), 
                                                                                           MetadataType.NewSchema));
        }
    }
}
