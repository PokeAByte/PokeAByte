using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Application
{
    /// <summary>
    /// The default implementation of the <see cref="IPokeAByteMapper"/> interface.
    /// </summary>
    public class PokeAByteMapper : IPokeAByteMapper, IDisposable
    {
        public PokeAByteMapper(
            MetadataSection metadata,
            MemorySection memory,
            IEnumerable<IPokeAByteProperty> properties,
            IEnumerable<ReferenceItems> references)
        {
            Metadata = metadata;
            Memory = memory;
            Properties = properties.ToDictionary(x => x.Path, x => x);
            References = references.ToDictionary(x => x.Name, x => x);
        }

        /// <inheritdoc />
        public MetadataSection Metadata { get; }
        
        /// <inheritdoc />
        public MemorySection Memory { get; }
        
        /// <inheritdoc />
        public Dictionary<string, IPokeAByteProperty> Properties { get; private set; }
        
        /// <inheritdoc />
        public Dictionary<string, ReferenceItems> References { get; private set;}

        // TODO: This is not exposed through the interface and currently unreferenced in the code. Can this be removed?
        public IPokeAByteProperty[] GetAllProperties() => Properties.Values.ToArray();

        public void Dispose()
        {
            Properties = [];
            References = [];
        }
    }
}