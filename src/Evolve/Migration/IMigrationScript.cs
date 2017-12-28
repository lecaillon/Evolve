using System.Collections.Generic;
using Evolve.Metadata;

namespace Evolve.Migration
{
    public interface IMigrationScript
    {
        MigrationVersion Version { get; }

        string Description { get; }

        string Name { get; }

        MetadataType Type { get; }

        string CheckSum { get; }

        IEnumerable<string> LoadSqlStatements(IDictionary<string, string> placeholders, string delimiter);

    }
}