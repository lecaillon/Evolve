using Evolve.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Evolve.Migration
{
    public sealed class MigrationVersion : IComparable<MigrationVersion>, IComparable
    {
        private const string InvalidObjectType = "Object must be of type MigrationVersion.";

        public MigrationVersion(string version)
        {
            Check.NotNullOrEmpty(version, nameof(version));
            
            Version = version.Replace('_', '.');
            if (!MatchPattern.IsMatch(Version)) throw new EvolveConfigurationException();

            VersionParts = Version.Split('.').Select(long.Parse).ToList();
        }

        public static Regex MatchPattern => new Regex("^[0-9]+(?:.[0-9]+)*$");

        public string Version { get; private set; }

        public List<long> VersionParts { get; set; }

        public int CompareTo(MigrationVersion other)
        {
            if (other == null) return 1;

            using (IEnumerator<long> e1 = VersionParts.GetEnumerator())
            using (IEnumerator<long> e2 = other.VersionParts.GetEnumerator())
            {
                while (e1.MoveNext())
                {
                    if (!e2.MoveNext())
                        return 1;

                    if (e1.Current.CompareTo(e2.Current) == 0)
                        continue;

                    return e1.Current.CompareTo(e2.Current);
                }
                return e2.MoveNext() ? -1 : 0;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj != null && !(obj is MigrationVersion))
                throw new ArgumentException(InvalidObjectType);

            return CompareTo(obj as MigrationVersion);
        }

        public override bool Equals(object obj) => (CompareTo(obj as MigrationVersion) == 0);

        public static bool operator >(MigrationVersion operand1, MigrationVersion operand2) => operand1.CompareTo(operand2) == 1;

        public static bool operator <(MigrationVersion operand1, MigrationVersion operand2) => operand1.CompareTo(operand2) == -1;

        public static bool operator >=(MigrationVersion operand1, MigrationVersion operand2) => operand1.CompareTo(operand2) >= 0;

        public static bool operator <=(MigrationVersion operand1, MigrationVersion operand2) => operand1.CompareTo(operand2) <= 0;

        public override int GetHashCode() => Version.GetHashCode();

        public override string ToString() => Version;
    }
}
