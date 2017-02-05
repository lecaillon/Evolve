using System;

namespace Evolve.Migration
{
    public class MigrationMetadata : MigrationBase
    {
        public MigrationMetadata(string version, string description, string name) : base(version, description, name)
        {
        }

        public int Id { get; set; }

        public string Checksum { get; set; }

        public string InstalledBy { get; set; }

        public DateTime InstalledOn { get; set; }

        public bool Success { get; set; }
    }
}
