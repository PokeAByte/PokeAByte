using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Interfaces;

public class ByteArrayJsonConverter : JsonConverter<byte[]>
{
    public override byte[] Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }
        reader.Read();

        var elements = new List<byte>();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            elements.Add(JsonSerializer.Deserialize<byte>(ref reader, options)!);

            reader.Read();
        }

        return elements.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (byte item in value)
        {
            JsonSerializer.Serialize(writer, item, options);
        }

        writer.WriteEndArray();
    }
}

public record PropertyAttributes
{
    public required string Path { get; init; }

    public required PropertyType Type { get; init; }
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

[JsonConverter(typeof(JsonStringEnumConverter<PropertyType>))]
public enum PropertyType : byte
{
    [JsonStringEnumMemberName("binaryCodedDecimal")]
    BinaryCodedDecimal,

    [JsonStringEnumMemberName("bitArray")]
    BitArray,

    [JsonStringEnumMemberName("bool")]
    Bool,

    [JsonStringEnumMemberName("bit")]
    Bit,

    [JsonStringEnumMemberName("int")]
    Int,

    [JsonStringEnumMemberName("string")]
    String,

    [JsonStringEnumMemberName("uint")]
    Uint
}

public interface IPokeAByteProperty
{
    [JsonPropertyName("path")]
    string Path { get; }

    [JsonPropertyName("type")]
    PropertyType Type { get; }

    [JsonPropertyName("memoryContainer")]
    string? MemoryContainer { get; }

    [JsonPropertyName("address")]
    uint? Address { get; }

    [JsonIgnore]
    string OriginalAddressString { get; }

    [JsonPropertyName("length")]
    int? Length { get; }

    [JsonPropertyName("size")]
    int? Size { get; }

    [JsonPropertyName("reference")]
    string? Reference { get; }

    [JsonPropertyName("bits")]
    string? Bits { get; }

    [JsonPropertyName("description")]
    string? Description { get; }

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    object? Value { get; set; }

    [JsonIgnore]
    object? FullValue { get; set; }

    [JsonPropertyName("bytes")]
    [JsonConverter(typeof(ByteArrayJsonConverter))]
    byte[]? Bytes { get; }

    [JsonIgnore]
    byte[]? BytesFrozen { get; }

    [JsonPropertyName("isFrozen")]
    bool IsFrozen { get; }

    [JsonPropertyName("isReadOnly")]
    bool IsReadOnly { get; }

    [JsonPropertyName("fieldsChanged")]
    HashSet<string> FieldsChanged { get; }

    void ProcessLoop(IPokeAByteInstance instance, IMemoryManager container, bool reloadAddresses);
    byte[] BytesFromBits(byte[] bytes);
    object? CalculateObjectValue(IPokeAByteInstance instance, byte[] bytes);
    //Exposing protected methods so we can maintian consistency
    byte[] BytesFromValue(string value, IPokeAByteMapper mapper);
    byte[] BytesFromFullValue(IPokeAByteMapper mapper);
}
