using System.IO;
using System.Text;

namespace Evolve.Migration
{
    public class EmbeddedResourceMigrationScript : MigrationScript
    {
        public EmbeddedResourceMigrationScript(string version, string description, string name, Stream content, Encoding encoding = null)
            : base(version, description, name, new StreamReader(content, encoding ?? Encoding.UTF8).ReadToEnd())
        {
        }
    }
}
