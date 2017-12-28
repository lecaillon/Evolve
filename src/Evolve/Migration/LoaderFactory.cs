using Evolve.Configuration;

namespace Evolve.Migration
{
    public static class LoaderFactory
    {
        public static IMigrationLoader GetLoader(IEvolveConfiguration config)
        {
            return config.EmbeddedResourceContext != null
                ? (IMigrationLoader) new EmbeddedResourceMigrationLoader(config.EmbeddedResourceContext)
                : new FileMigrationLoader();
        }
    }
}