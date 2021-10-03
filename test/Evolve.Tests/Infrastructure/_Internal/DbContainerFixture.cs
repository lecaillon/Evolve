using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace EvolveDb.Tests.Infrastructure
{
    public interface IDbContainerFixture
    {
        string CnxStr { get; }
        void Run(bool fromScratch = false);
        DbConnection CreateDbConnection();
        void Dispose();
    }

    public abstract class DbContainerFixture<T> : IDbContainerFixture where T : IDbContainer, new()
    {
        protected readonly T _container = new();

        public string CnxStr => _container.CnxStr;

        [SuppressMessage("Design", "CA1031: Do not catch general exception types")]
        public virtual void Run(bool fromScratch = false)
        {
            int retries = 1;
            bool isDbStarted = false;

            _container.Start(fromScratch);

            while (!isDbStarted)
            {
                if (retries > _container.TimeOutInSec)
                {
                    throw new Exception($"{typeof(T).Name} timed-out after {_container.TimeOutInSec} sec.");
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
                try
                {
                    using var cnn = CreateDbConnection();
                    cnn.Open();
                    isDbStarted = cnn.State == ConnectionState.Open;
                }
                catch { }
                retries++;
            }
        }

        public DbConnection CreateDbConnection() => _container.CreateDbConnection();

        public void Dispose() => _container.Dispose();
    }
}
