using System;
using System.IO;
using System.Text;

namespace Evolve.Migration
{
    public class EmbeddedResourceMigrationScript: MigrationScript
    {
        public EmbeddedResourceMigrationScript(
            string version,
            string name,
            string description,
            Func<Stream> migrationStream,
            Encoding encoding = null,
            bool normalizeLineEndings = false)
            : base(version, name, description, migrationStream, encoding, normalizeLineEndings)
        {
        }
    }
}