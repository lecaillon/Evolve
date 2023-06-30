using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EvolveDb.Tests.Infrastructure
{
    public abstract class DbContainerFixture<T> : IAsyncLifetime where T : IDbContainer, new()
    {
        private static readonly SemaphoreSlim Semaphore = new(1);

        protected readonly T _container = new();

        public string CnxStr => _container.CnxStr;
        public virtual bool FromScratch { get; } = false;
        public virtual bool MustRunContainer { get; } = true;
        public virtual Action Initialize { get; }

        public DbConnection CreateDbConnection() => _container.CreateDbConnection();

        public async Task InitializeAsync()
        {
            await Semaphore.WaitAsync();

            if (!MustRunContainer)
            {
                return;
            }

            bool wasAlreadyStarted = !await _container.Start(FromScratch);

            int retries = 1;
            bool isDbStarted = false;
            while (!isDbStarted)
            {
                if (retries > _container.TimeOutInSec)
                {
                    throw new Exception($"{typeof(T).Name} timed-out after {_container.TimeOutInSec} sec.");
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                try
                {
                    using var cnn = CreateDbConnection();
                    cnn.Open();
                    isDbStarted = cnn.State == ConnectionState.Open;
                }
                catch { }
                retries++;
            }

            if (!wasAlreadyStarted)
            {
                // Extra margin before executing queries
                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            Initialize?.Invoke();
        }

        public Task DisposeAsync()
        {
            Semaphore.Release();
            return Task.CompletedTask;
        }
    }
}
