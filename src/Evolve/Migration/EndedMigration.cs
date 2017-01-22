using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Migration
{
    public class EndedMigration : MigrationBase
    {
        int Id { get; set; }

        string Checksum { get; set; }

        string UserInstall { get; set; }

        DateTime DateInstall { get; set; }

        bool Success { get; set; }
    }
}
