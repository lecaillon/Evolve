using Evolve.Connection;
using Evolve.Utilities;

namespace Evolve.Dialect
{
    public abstract class Schema
    {
        protected readonly WrappedConnection _wrappedConnection;

        public Schema(string schemaName, WrappedConnection wrappedConnection)
        {
            Name = Check.NotNullOrEmpty(schemaName, nameof(schemaName));
            _wrappedConnection = Check.NotNull(wrappedConnection, nameof(wrappedConnection));
        }

        public string Name { get; }

        public abstract bool IsExists();

        public abstract bool IsEmpty();

        public abstract bool Create();

        public abstract bool Erase();

        public abstract bool Drop();
    }
}
