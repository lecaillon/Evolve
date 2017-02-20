using Evolve.Metadata;
using Evolve.Utilities;
using System;

namespace Evolve.Migration
{
    public abstract class MigrationBase : IComparable<MigrationBase>, IComparable
    {
        private const string InvalidObjectType = "Object must be of type MigrationBase.";

        public MigrationBase(string version, string description, string name, MetadataType type)
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

        public int CompareTo(MigrationBase other)
        {
            if (other == null) return 1;

            return Version.CompareTo(other.Version);
        }

        public int CompareTo(object obj)
        {
            if (obj != null && !(obj is MigrationBase))
                throw new ArgumentException(InvalidObjectType);

            return Version.CompareTo((obj as MigrationBase).Version);
        }

        public override bool Equals(object obj) => (CompareTo(obj as MigrationBase) == 0);

        public static bool operator ==(MigrationBase operand1, MigrationBase operand2)
        {
            if (ReferenceEquals(operand1, null))
            {
                return ReferenceEquals(operand2, null);
            }

            return operand1.Equals(operand2);
        }

        public static bool operator !=(MigrationBase operand1, MigrationBase operand2) => !(operand1 == operand2);

        public static bool operator >(MigrationBase operand1, MigrationBase operand2) => operand1.CompareTo(operand2) == 1;

        public static bool operator <(MigrationBase operand1, MigrationBase operand2) => operand1.CompareTo(operand2) == -1;

        public static bool operator >=(MigrationBase operand1, MigrationBase operand2) => operand1.CompareTo(operand2) >= 0;

        public static bool operator <=(MigrationBase operand1, MigrationBase operand2) => operand1.CompareTo(operand2) <= 0;

        public override int GetHashCode() => Version.GetHashCode();

        public override string ToString() => Name;
    }
}
