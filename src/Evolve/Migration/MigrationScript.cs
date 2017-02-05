using System;
using System.IO;
using System.Security.Cryptography;
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

        public string CalculateChecksum()
        {
            using (var md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(Path))
                {
                    byte[] checksum = md5.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
            }
        }
    }
}
