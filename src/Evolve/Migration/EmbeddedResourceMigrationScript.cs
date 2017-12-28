using System;
using System.IO;

namespace Evolve.Migration
{
    public class EmbeddedResourceMigrationScript: MigrationScript
    {
        public EmbeddedResourceMigrationScript(string version, string name, string description, Func<TextReader> migrationStream)
            : base(version, name, description, migrationStream)
        {
        }
    }
}