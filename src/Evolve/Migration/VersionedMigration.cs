using System;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public abstract class VersionedMigration : IComparable<VersionedMigration>, IComparable
    {
        private const string InvalidObjectType = "Object must be of type MigrationBase.";

        public VersionedMigration(string version, string description, string name, MetadataType type)
        {
            Description = Check.NotNullOrEmpty(description, nameof(description));
            Name = Check.NotNull(name, nameof(name));
            Version = new MigrationVersion(Check.NotNullOrEmpty(version, nameof(version)));
            Type = type;
        }

        public MigrationVersion Version { get; }

        public string Description { get; }

        public string Name { get; }

        public MetadataType Type { get; }

        public int CompareTo(VersionedMigration other)
        {
            if (other == null) return 1;

            return Version.CompareTo(other.Version);
        }

        public int CompareTo(object obj)
        {
            if (obj != null && !(obj is VersionedMigration))
                throw new ArgumentException(InvalidObjectType);

            return Version.CompareTo((obj as VersionedMigration).Version);
        }

        public override bool Equals(object obj) => (CompareTo(obj as VersionedMigration) == 0);

        public static bool operator ==(VersionedMigration operand1, VersionedMigration operand2)
        {
            if (ReferenceEquals(operand1, null))
            {
                return ReferenceEquals(operand2, null);
            }

            return operand1.Equals(operand2);
        }

        public static bool operator !=(VersionedMigration operand1, VersionedMigration operand2) => !(operand1 == operand2);

        public static bool operator >(VersionedMigration operand1, VersionedMigration operand2) => operand1.CompareTo(operand2) == 1;

        public static bool operator <(VersionedMigration operand1, VersionedMigration operand2) => operand1.CompareTo(operand2) == -1;

        public static bool operator >=(VersionedMigration operand1, VersionedMigration operand2) => operand1.CompareTo(operand2) >= 0;

        public static bool operator <=(VersionedMigration operand1, VersionedMigration operand2) => operand1.CompareTo(operand2) <= 0;

        public override int GetHashCode() => Version.GetHashCode();

        public override string ToString() => Name;
    }
}
