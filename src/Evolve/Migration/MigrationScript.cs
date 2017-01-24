using Evolve.Utilities;

namespace Evolve.Migration
{
    public class MigrationScript : MigrationBase
    {
        public MigrationScript(string path, string version, string description) 
            : base(version, description, System.IO.Path.GetFileNameWithoutExtension(Check.FileExists(path, nameof(path))))
        {
            Path = path;
        }

        public string Path { get; set; }
    }
}
