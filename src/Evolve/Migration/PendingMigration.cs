using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Migration
{
    public class PendingMigration : MigrationBase
    {
        public PendingMigration(string version, string description, string name) : base(version, description, name)
        {
        }

        public string Path { get; set; }
    }
}
