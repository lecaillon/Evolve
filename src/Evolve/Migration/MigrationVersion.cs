using Evolve.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Evolve.Migration
{
    public class MigrationVersion
    {
        public MigrationVersion(string version)
        {
            Check.NotNullOrEmpty(version, nameof(version));
            
            Version = version.Replace('_', '.');
            if (!MatchPattern.IsMatch(Version)) throw new EvolveConfigurationException();

            VersionParts = Version.Split('.').Select(int.Parse).ToList();
        }

        public static Regex MatchPattern => new Regex("^[0-9]+(?:.[0-9]+)*$");

        public string Version { get; private set; }

        public List<int> VersionParts { get; set; }
    }
}
