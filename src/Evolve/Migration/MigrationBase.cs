using System;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    internal abstract class MigrationBase : IComparable<MigrationBase>, IComparable
    {
        private const string InvalidObjectType = "Object must be of type MigrationBase.";

        protected MigrationBase(string? version, string description, string name, MetadataType type)
        {
            Description = Check.NotNullOrEmpty(description, nameof(description));
            Name = Check.NotNull(name, nameof(name));
            Type = type;
            Version = Type == MetadataType.RepeatableMigration
                ? null
                : new MigrationVersion(Check.NotNullOrEmpty(version, nameof(version)));
        }

        /// <summary>
        ///     Returns the version of the migration.
        ///     Can be null in case of <see cref="MetadataType.RepeatableMigration"/>.
        /// </summary>
        public MigrationVersion? Version { get; }

        public string Description { get; }

        public string Name { get; }

        public MetadataType Type { get; }

        public int CompareTo(MigrationBase? other)
        {
            if (other is null) return 1;
            if (Version is null && other.Version != null) return 1;
            if (Version != null && other.Version is null) return -1;
            return Version is null
                ? Name.CompareTo(other.Name)
                : Version.CompareTo(other.Version);
        }

        public int CompareTo(object? obj)
        {
            if (obj != null && !(obj is MigrationBase)) throw new ArgumentException(InvalidObjectType);
            if (Version is null && (obj as MigrationBase)?.Version != null) return 1;
            if (Version != null && (obj as MigrationBase)?.Version is null) return -1;
            return Version is null
                ? Name.CompareTo((obj as MigrationBase)?.Name)
                : Version.CompareTo((obj as MigrationBase)?.Version);
        }

        public override bool Equals(object? obj) => (CompareTo(obj as MigrationBase) == 0);

        public static bool operator ==(MigrationBase? operand1, MigrationBase? operand2)
        {
            if (operand1 is null)
            {
                return operand2 is null;
            }

            return operand1.Equals(operand2);
        }

        public static bool operator !=(MigrationBase? operand1, MigrationBase? operand2) => !(operand1 == operand2);

        public static bool operator >(MigrationBase? operand1, MigrationBase? operand2) => operand1?.CompareTo(operand2) == 1;

        public static bool operator <(MigrationBase? operand1, MigrationBase? operand2) => operand1?.CompareTo(operand2) == -1;

        public static bool operator >=(MigrationBase? operand1, MigrationBase? operand2) => operand1?.CompareTo(operand2) >= 0;

        public static bool operator <=(MigrationBase? operand1, MigrationBase? operand2) => operand1?.CompareTo(operand2) <= 0;

        public override int GetHashCode() => Version is null ? Name.GetHashCode() : Version.GetHashCode();

        public override string ToString() => Name;
    }
}
