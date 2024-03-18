namespace GameHook.Domain
{
    public enum EndianTypes
    {
        BigEndian,
        LittleEndian
    }

    public record MemoryAddressBlock
    {
        public MemoryAddressBlock(string name, MemoryAddress startingAddress, MemoryAddress endingAddress)
        {
            Name = name;
            StartingAddress = startingAddress;
            EndingAddress = endingAddress;
        }

        public string Name { get; init; }
        public MemoryAddress StartingAddress { get; init; }
        public MemoryAddress EndingAddress { get; init; }
    }

    public class ReferenceItems
    {
        public string Name { get; init; } = string.Empty;
        public string? Type { get; init; }

        public IEnumerable<ReferenceItem> Values { get; init; } = new List<ReferenceItem>();

        public ReferenceItem? GetSingleOrDefaultByKey(ulong key)
        {
            return Values.SingleOrDefault(x => x.Key == key);
        }

        public ReferenceItem GetFirstByValue(object? value)
        {
            return Values.FirstOrDefault(x => string.Equals(x.Value?.ToString(), value?.ToString(), StringComparison.Ordinal)) ?? throw new Exception($"Missing dictionary value for '{value}', value was not found in reference list {Name}.");
        }
    }

    public record ReferenceItem(ulong Key, object? Value);
}
