using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Application
{
    public class PokeAByteMapper : IPokeAByteMapper
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

        public MetadataSection Metadata { get; }
        public MemorySection Memory { get; }
        public Dictionary<string, IPokeAByteProperty> Properties { get; }
        public Dictionary<string, ReferenceItems> References { get; }

        public IPokeAByteProperty[] GetAllProperties() => Properties.Values.ToArray();
    }
}