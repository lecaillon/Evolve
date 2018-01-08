using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class FileMigrationScript : MigrationScript
    {
        public FileMigrationScript(string path, string version, string description, Encoding encoding = null, bool normalizeLineEndings = false) :
            base(
                version,
                System.IO.Path.GetFileName(Check.FileExists(path, nameof(path))),
                description,
                () => File.OpenRead(path),
                encoding,
                normalizeLineEndings
                )
        {
            Path = path;
        }
        public string Path { get; set; }

    }
}
