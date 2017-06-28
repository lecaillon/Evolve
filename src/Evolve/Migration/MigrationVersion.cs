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
        private const string InvalidVersionPatternMatching = "Migration version {0} is invalid. Version must respect this regex: ^[0-9]+(?:.[0-9]+)*$";

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="version"> Version of the script. Example: 1_1_0_11 </param>
        /// <exception cref="EvolveConfigurationException"> Thrown when the format of the version is invalid. </exception>
        public MigrationVersion(string version)
        {
            Check.NotNullOrEmpty(version, nameof(version));
            
            Label = version.Replace('_', '.');
            if (!MatchPattern.IsMatch(Label))
                throw new EvolveConfigurationException(string.Format(InvalidVersionPatternMatching, Label));

            VersionParts = Label.Split('.').Select(long.Parse).ToList();
        }

        public static Regex MatchPattern => new Regex("^[0-9]+(?:.[0-9]+)*$");

        public string Label { get; }

        public List<long> VersionParts { get; set; }

        public static MigrationVersion MinVersion => new MigrationVersion("0");

        #region IComparable

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

        #endregion

        #region Operators

        public override bool Equals(object obj) => (CompareTo(obj as MigrationVersion) == 0);

        public static bool operator ==(MigrationVersion operand1, MigrationVersion operand2)
        {
            if (ReferenceEquals(operand1, null))
            {
                return ReferenceEquals(operand2, null);
            }

            return operand1.Equals(operand2);
        }

        public static bool operator !=(MigrationVersion operand1, MigrationVersion operand2) => !(operand1 == operand2);

        public static bool operator >(MigrationVersion operand1, MigrationVersion operand2) => operand1.CompareTo(operand2) == 1;

        public static bool operator <(MigrationVersion operand1, MigrationVersion operand2) => operand1.CompareTo(operand2) == -1;

        public static bool operator >=(MigrationVersion operand1, MigrationVersion operand2) => operand1.CompareTo(operand2) >= 0;

        public static bool operator <=(MigrationVersion operand1, MigrationVersion operand2) => operand1.CompareTo(operand2) <= 0;

        #endregion

        public override int GetHashCode() => Label.GetHashCode();

        public override string ToString() => Label;
    }
}
