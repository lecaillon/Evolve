using System;
using System.Threading;

namespace Evolve.Tests.Infrastructure
{
    public class CockroachDBFixture : DbContainerFixture<CockroachDBContainer>, IDisposable
    {
        public override void Run(bool fromScratch = false)
        {
            base.Run(fromScratch);

            // Extra margin before executing queries
            if (fromScratch)
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}
