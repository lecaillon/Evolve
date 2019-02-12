using System;
using System.Threading;

namespace Evolve.Tests.Infrastructure
{
    public class PostgreSqlFixture : DbContainerFixture<PostgreSqlContainer>, IDisposable
    {
        public override void Run(bool fromScratch = false)
        {
            base.Run(fromScratch);

            // Extra margin before executing queries
            if (fromScratch)
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }
    }
}
