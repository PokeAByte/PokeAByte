namespace PokeAByte.Domain.Interfaces;

public record PropertyAttributes
{
    public required string Path { get; init; }

    public required string Type { get; init; }
    public string? MemoryContainer { get; init; }
    public string? Address { get; init; }
    public int? Length { get; init; } = 1;
    public int? Size { get; init; }
    public string? Bits { get; set; }
    public string? Reference { get; set; }
    public string? Description { get; set; }

    public string? Value { get; set; }
    public string? ReadFunction { get; set; }
    public string? WriteFunction { get; set; }

    public string? AfterReadValueExpression { get; set; }
    public string? AfterReadValueFunction { get; set; }

    public string? BeforeWriteValueFunction { get; set; }
    public EndianTypes EndianType { get; set; }
}

public interface IPokeAByteProperty
{
    string Path { get; }
    string Type { get; }
    string? MemoryContainer { get; }
    uint? Address { get; }
    string OriginalAddressString { get; }
    int? Length { get; }
    int? Size { get; }
    string? Reference { get; }
    string? Bits { get; }
    string? Description { get; }

    object? Value { get; set; }
    object? FullValue { get; set; }
    byte[]? Bytes { get; }
    byte[]? BytesFrozen { get; }

    bool IsFrozen { get; }
    bool IsReadOnly { get; }

    HashSet<string> FieldsChanged { get; }

    void ProcessLoop(IPokeAByteInstance instance, IMemoryManager container, bool reloadAddresses);
    byte[] BytesFromBits(byte[] bytes);
    object? CalculateObjectValue(IPokeAByteInstance instance, byte[] bytes);
    Task WriteValue(string value, bool? freeze);
    Task WriteBytes(byte[] bytes, bool? freeze);
    Task FreezeProperty(byte[] bytesFrozen);
    Task UnfreezeProperty();
    //Exposing protected methods so we can maintian consistency
    object? ObjectFromBytes(byte[] value);
    byte[] BytesFromValue(string value);
    byte[] BytesFromFullValue();
    //public string? AfterReadValueExpression { get; set; }
}
