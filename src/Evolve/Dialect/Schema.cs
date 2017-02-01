using Evolve.Connection;
using Evolve.Utilities;

namespace Evolve.Dialect
{
    public abstract class Schema
    {
        protected DatabaseHelper _dbHelper;
        protected IWrappedConnection _wrappedConnection;

        public Schema(string schemaName, DatabaseHelper dbHelper)
        {
            Name = Check.NotNullOrEmpty(schemaName, nameof(schemaName));
            _dbHelper = Check.NotNull(dbHelper, nameof(dbHelper));
            _wrappedConnection = dbHelper.WrappedConnection;
        }

        public string Name { get; private set; }

        protected abstract bool IsExists();

        protected abstract bool IsEmpty();

        protected abstract bool Create();

        protected abstract bool Clean();

        protected abstract bool Drop();
    }
}
