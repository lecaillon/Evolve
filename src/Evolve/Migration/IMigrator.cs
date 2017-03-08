namespace Evolve.Migration
{
    public interface IMigrator
    {
        void Migrate(string targetVersion = null);

        string GenerateScript(string fromMigration = null, string toMigration = null);

        void Validate();

        void Erase();
    }
}
