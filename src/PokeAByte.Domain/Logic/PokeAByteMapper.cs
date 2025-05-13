using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Logic;

public class PokeAByteMapper : IPokeAByteMapper, IDisposable
{
    public PokeAByteMapper(
        MetadataSection metadata,
        IPlatformOptions platformOptions,
        MemorySection memory,
        IEnumerable<IPokeAByteProperty> properties,
        IEnumerable<ReferenceItems> references
    )
    {
        Metadata = metadata;
        Memory = memory;
        Properties = properties.ToDictionary(x => x.Path, x => x);
        References = references.ToDictionary(x => x.Name, x => x);
        PlatformOptions = platformOptions;
    }

    public MetadataSection Metadata { get; }
    public MemorySection Memory { get; }
    public Dictionary<string, IPokeAByteProperty> Properties { get; private set; }
    public Dictionary<string, ReferenceItems> References { get; private set; }
    public IPlatformOptions PlatformOptions { get; private set; }

    public IPokeAByteProperty[] GetAllProperties() => Properties.Values.ToArray();

    public void Dispose()
    {
        Properties = [];
        References = [];
    }
}