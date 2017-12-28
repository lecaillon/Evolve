using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class FileMigrationScript : MigrationScript
    {
        public FileMigrationScript(string path, string version, string description, Encoding encoding = null) :
            base(
                version,
                description,
                System.IO.Path.GetFileName(Check.FileExists(path, nameof(path))),
                () => new StreamReader(File.OpenRead(path), encoding ?? Encoding.UTF8))
        {
            Path = path;
        }
        public string Path { get; set; }

    }
}
