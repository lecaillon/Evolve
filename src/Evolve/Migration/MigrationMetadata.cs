using System;
using System.IO;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class MigrationMetadata : MigrationBase
    {
        public MigrationMetadata(string version, string description, string name) 
            : base(version, description, Path.GetFileNameWithoutExtension(Check.NotNullOrEmpty(name, nameof(name))))
        {
        }

        public int Id { get; set; }

        public string Checksum { get; set; }

        public string InstalledBy { get; set; }

        public DateTime InstalledOn { get; set; }

        public bool Success { get; set; }
    }
}
