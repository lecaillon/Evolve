using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Core.Driver
{
    public class NpgsqlDriver : ReflectionBasedDriver
    {
        public const string DriverAssemblyName = "Npgsql";
        public const string ConnectionTypeName = "Npgsql.NpgsqlConnection";

        public NpgsqlDriver() : base(DriverAssemblyName, ConnectionTypeName)
        {
        }
    }
}
