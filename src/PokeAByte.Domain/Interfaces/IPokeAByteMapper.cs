namespace PokeAByte.Domain.Interfaces
{
    public class MetadataSection
    {
        public Guid Id { get; init; } = Guid.Empty;
        public string GameName { get; init; } = string.Empty;
        public string GamePlatform { get; init; } = string.Empty;
    }

    public class MemorySection
    {
        public ReadRange[] ReadRanges { get; init; } = [];
    }

    public class ReadRange
    {
        public uint Start { get; init; }
        public uint End { get; init; }
    }

    public interface IPokeAByteMapper : IDisposable
    {
        MetadataSection Metadata { get; }
        MemorySection Memory { get; }
        Dictionary<string, IPokeAByteProperty> Properties { get; }
        Dictionary<string, ReferenceItems> References { get; }
    }
}
