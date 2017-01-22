using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Migration
{
    public class MigrationScript : MigrationBase
    {
        public MigrationScript(string version, string description, string name) : base(version, description, name)
        {
            // penser à gérer les exceptions de configuration
        }

        public string Path { get; set; }
    }
}
