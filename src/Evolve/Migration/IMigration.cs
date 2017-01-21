using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Migration
{
    public interface IMigration
    {
        int? Id { get; set; }

        MigrationVersion Version { get; set; }

        string Description { get; set; }

        string Checksum { get; set; }

        string UserInstall { get; set; }

        DateTime DateInstall { get; set; }

        bool Success { get; set; }
    }
}
