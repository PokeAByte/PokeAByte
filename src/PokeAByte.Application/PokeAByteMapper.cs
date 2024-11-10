using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Application
{

    public class PokeAByteMapper : IPokeAByteMapper, IDisposable
    {
        public PokeAByteMapper(
            MetadataSection metadata,
            MemorySection memory,
            IEnumerable<IPokeAByteProperty> properties,
            IEnumerable<ReferenceItems> references,
            IList<MapperVariable> variables
        )
        {
            Metadata = metadata;
            Memory = memory;
            Properties = properties.ToDictionary(x => x.Path, x => x);
            References = references.ToDictionary(x => x.Name, x => x);
            Variables = variables;
        }

        public MetadataSection Metadata { get; }
        public MemorySection Memory { get; }
        public Dictionary<string, IPokeAByteProperty> Properties { get; private set; }
        public Dictionary<string, ReferenceItems> References { get; private set;}
        public IList<MapperVariable> Variables { get; private set;}

        public IPokeAByteProperty[] GetAllProperties() => Properties.Values.ToArray();

        public void Dispose()
        {
            Properties = [];
            References = [];
        }
    }
}