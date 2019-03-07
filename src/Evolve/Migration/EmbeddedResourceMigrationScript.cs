using System;
using System.IO;
using System.Text;
using Evolve.Metadata;

namespace Evolve.Migration
{
    public class EmbeddedResourceMigrationScript : MigrationScript
    {
        private const string IncorrectMigrationType = "Embedded resource migration {0} must be of type MetadataType.Migration or MetadataType.RepeatableMigration";

        /// <summary>
        ///     Initialize a new versioned embedded migration
        /// </summary>
        public EmbeddedResourceMigrationScript(string version, string description, string name, Stream content, Encoding encoding = null)
            : base(version, 
                   description, 
                   name, 
                   content: new StreamReader(content, encoding ?? Encoding.UTF8).ReadToEnd(),
                   type: MetadataType.Migration)
        {
        }

        /// <summary>
        ///     Initialize a new repeatable embedded migration
        /// </summary>
        public EmbeddedResourceMigrationScript(string description, string name, Stream content, Encoding encoding = null)
            : base(version: null,
                   description,
                   name,
                   content: new StreamReader(content, encoding ?? Encoding.UTF8).ReadToEnd(),
                   type: MetadataType.RepeatableMigration)
        {
        }
    }
}
