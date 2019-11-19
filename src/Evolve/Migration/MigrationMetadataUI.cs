using System;

namespace Evolve.Migration
{
    public class MigrationMetadataUI
    {
        public MigrationMetadataUI(string? version, string description, string category)
        {
            Version = version;
            Description = description;
            Category = category;
        }

        internal MigrationMetadataUI(MigrationMetadata migration)
        {
            Id = migration.Id;
            Version = migration.Version?.Label;
            Category = migration.Type.ToString();
            Description = migration.Description;
            InstalledBy = migration.InstalledBy;
            InstalledOn = migration.InstalledOn;
            Success = migration.Success;
            Checksum = migration.Checksum;
        }

        public int? Id { get; set; }
        public string? Version { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? InstalledBy { get; set; }
        public DateTime? InstalledOn { get; set; }
        public bool? Success { get; set; }
        public string? Checksum { get; set; }
    }
}