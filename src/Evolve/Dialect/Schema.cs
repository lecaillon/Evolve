using Evolve.Utilities;

namespace Evolve.Dialect
{
    public abstract class Schema
    {
        public Schema(string schemaName, DatabaseHelper databaseHelper)
        {
            Check.NotNullOrEmpty(schemaName, nameof(schemaName));
            Check.NotNull(databaseHelper, nameof(databaseHelper));

            Name = schemaName;
            DatabaseHelper = databaseHelper;
        }

        public string Name { get; private set; }

        protected DatabaseHelper DatabaseHelper { get; private set; }

        public bool IsExists()
        {
            // + gestion exception
            return InternalIsExists();
        }

        public bool IsEmpty()
        {
            // + gestion exception
            return InternalIsEmpty();
        }

        public void Create()
        {
            // + gestion exception
            InternalCreate();
        }

        public void Clean()
        {
            // + gestion exception
            InternalClean();
        }

        public void Drop()
        {
            // + gestion exception
            InternalDrop();
        }

        protected abstract bool InternalIsExists();

        protected abstract bool InternalIsEmpty();

        protected abstract bool InternalCreate();

        protected abstract bool InternalClean();

        protected abstract bool InternalDrop();
    }
}
