using System;
using System.Collections.Generic;
using System.Text;

namespace Evolve.Configuration
{
    public class JsonConfigurationProvider : EvolveConfigurationProviderBase
    {
        protected override void Configure()
        {
            _configuration.Command = CommandOptions.Erase;

            //_configuration.Driver = "npgsql";
            //_configuration.ConnectionString = "Server=127.0.0.1;Port=5432;Database=my_database;User Id=postgres;Password=Password12!;";

            _configuration.Driver = "microsoftdatasqlite";
            _configuration.ConnectionString = "Data Source=:memory:;";
        }
    }
}
