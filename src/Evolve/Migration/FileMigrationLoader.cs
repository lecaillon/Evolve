using System;
using System.Collections.Generic;
using System.IO;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class FileMigrationLoader : IMigrationLoader
    {
        private const string InvalidSqlScriptLocation = "Invalid sql script location: {0}.";

        public IEnumerable<MigrationScript> GetMigrations(IEnumerable<string> locations, string prefix, string separator, string suffix)
        {
            Check.HasNoNulls(locations, nameof(locations));
            Check.NotNullOrEmpty(prefix, nameof(prefix));
            Check.NotNullOrEmpty(separator, nameof(separator));
            Check.NotNullOrEmpty(suffix, nameof(suffix));

            DirectoryInfo dirToScan = null;
            foreach (string location in locations)
            {
                dirToScan = ResolveDirectory(location);
                dirToScan.GetFiles("", SearchOption.AllDirectories);
            }
        }

        private DirectoryInfo ResolveDirectory(string location)
        {
            Check.NotNullOrEmpty(location, nameof(location));

            try
            {
                return new DirectoryInfo(location);
            }
            catch (Exception ex)
            {
                throw new EvolveConfigurationException(string.Format(InvalidSqlScriptLocation, location), ex);
            }
        }
    }
}
