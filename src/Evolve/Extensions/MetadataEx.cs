using System.Linq;
using Evolve.Metadata;

namespace Evolve
{
    internal static class MetadataEx
    {
        public static bool IsEvolveInitialized(this IEvolveMetadata metadata)
        {
            try
            {
                return metadata.IsExists();
            }
            catch
            {
                return false;
            }
        }

        public static bool IsEmptySchemaMetadataExists(this IEvolveMetadata metadata, string schemaName)
            => IsEvolveInitialized(metadata) && metadata.GetAllMetadata().Any(x => x.Type == MetadataType.EmptySchema && x.Name == schemaName);
    }
}
