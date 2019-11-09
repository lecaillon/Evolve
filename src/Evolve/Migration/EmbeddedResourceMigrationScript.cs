using System;
using System.IO;
using System.Text;
using Evolve.Metadata;

namespace Evolve.Migration
{
    internal class EmbeddedResourceMigrationScript : MigrationScript
    {
        private const string IncorrectMigrationType = "Embedded resource migration {0} must be of type MetadataType.Migration or MetadataType.RepeatableMigration";

        public EmbeddedResourceMigrationScript(string version, string description, string name, Stream content, MetadataType type, Encoding encoding = null)
            : base(version, 
                   description, 
                   name, 
                   content: new StreamReader(content, encoding ?? Encoding.UTF8).ReadToEnd(),
                   type == MetadataType.Migration || type == MetadataType.RepeatableMigration 
                       ? type 
                       : throw new NotSupportedException(string.Format(IncorrectMigrationType, name)))
        {
        }
    }
}
