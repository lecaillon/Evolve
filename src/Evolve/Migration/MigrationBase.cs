using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Migration
{
    public abstract class MigrationBase : IComparable<MigrationBase>, IComparable
    {
        private const string InvalidObjectType = "Object must be of type MigrationBase.";

        public MigrationBase()
        {

        }

        public MigrationBase(string version, string description, string name)
        {

        }

        MigrationVersion Version { get; set; }

        string Description { get; set; }

        string Name { get; set; }

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

        public static bool operator >(MigrationBase operand1, MigrationBase operand2) => operand1.CompareTo(operand2) == 1;

        public static bool operator <(MigrationBase operand1, MigrationBase operand2) => operand1.CompareTo(operand2) == -1;

        public static bool operator >=(MigrationBase operand1, MigrationBase operand2) => operand1.CompareTo(operand2) >= 0;

        public static bool operator <=(MigrationBase operand1, MigrationBase operand2) => operand1.CompareTo(operand2) <= 0;

        public override int GetHashCode() => Version.GetHashCode();

        public override string ToString() => Name;
    }
}
