using System;
using System.Data;
using System.Threading;

namespace Evolve.Tests.Infrastructure
{
    public abstract class DbContainerFixture<T> where T : IDbContainer, new()
    {
        protected readonly T _container = new T();

        public string CnxStr => _container.CnxStr;

        public virtual void Run(bool fromScratch = false)
        {
            int retries = 1;
            bool isDbStarted = false;

            _container.Start(fromScratch);

            if (typeof(T) == typeof(CassandraContainer) && fromScratch)
            {
                Thread.Sleep(TimeSpan.FromSeconds(_container.TimeOutInSec));
            }

            while (!isDbStarted)
            {
                if (retries > _container.TimeOutInSec)
                {
                    throw new Exception($"{typeof(T).Name} timed-out after {_container.TimeOutInSec} sec.");
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
                try
                {
                    using (var cnn = CreateDbConnection())
                    {
                        cnn.Open();
                        isDbStarted = cnn.State == ConnectionState.Open;
                    }
                }
                catch { }
                retries++;
            }
        }

        public IDbConnection CreateDbConnection() => _container.CreateDbConnection();

        public void Dispose() => _container.Dispose();
    }
}
