﻿using System.Data;
using System.Diagnostics.CodeAnalysis;
using Npgsql;

namespace Evolve.Tests.Infrastructure
{
    public class CockroachDBContainer : IDbContainer
    {
        public const string ExposedPort = "26257";
        public const string HostPort = "26257";
        public const string DbName = "defaultdb";

        private DockerContainer _container;
        private bool _disposedValue = false;

        public string Id => _container?.Id;
        public string CnxStr => $"Host=localhost;Username=root;Port={HostPort};Database={DbName};";
        public int TimeOutInSec => 8;

        [SuppressMessage("Qualité du code", "IDE0068: Utilisez le modèle de suppression recommandé")]
        public bool Start(bool fromScratch = false)
        {
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "cockroachdb/cockroach",
                Tag = "latest",
                Name = "cockroachdb-evolve",
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort,
                RemovePreviousContainer = fromScratch,
                Cmd = new[] { "start", "--insecure" }
            }).Build();

            return _container.Start();
        }

        public IDbConnection CreateDbConnection() => new NpgsqlConnection(CnxStr);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _container?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
