using System;
using Evolve.Metadata;

namespace Evolve.Migration
{
    internal class MigrationMetadata : MigrationBase
    {
        public MigrationMetadata(string? version, string description, string name, MetadataType type) 
            : base(version, description, name, type)
        {
        }

        public int Id { get; set; }
        public string? Checksum { get; set; }
        public string? InstalledBy { get; set; }
        public DateTime InstalledOn { get; set; }
        public bool Success { get; set; }
    }
}
