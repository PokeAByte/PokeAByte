using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Interfaces;

[JsonSerializable(typeof(byte))]
public partial class ConverterCOntext : JsonSerializerContext;

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
            elements.Add(JsonSerializer.Deserialize(ref reader, ConverterCOntext.Default.Byte)!);

            reader.Read();
        }

        return elements.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (byte item in value)
        {
            JsonSerializer.Serialize(writer, item, ConverterCOntext.Default.Byte);
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
    /// <summary>
    /// The unique path of the property.
    /// </summary>
    [JsonPropertyName("path")]
    string Path { get; }

    /// <summary>
    /// The datatype of the property.
    /// </summary>
    [JsonPropertyName("type")]
    PropertyType Type { get; }

    /// <summary>
    /// Identifies the <see cref="IMemoryNamespace"/> the raw property memory is stored in. <br/>
    /// If <see langword="null"/>, then the <see cref="IMemoryManager.DefaultNamespace"/> is used.
    /// </summary>
    [JsonPropertyName("memoryContainer")]
    string? MemoryContainer { get; }

    /// <summary>
    /// The calculated game memory address from which the property bytes are read.
    /// </summary>
    [JsonPropertyName("address")]
    uint? Address { get; }

    /// <summary>
    /// The raw expression string from the mapper XML from which the <see cref="Address"/> is calculated.
    /// </summary>
    [JsonIgnore]
    string OriginalAddressString { get; }

    /// <summary>
    /// The number bytes that makes up the the property data. <br/>
    /// The mapper XML parser defaults to <c>1</c> if the length attribute is omitted.
    /// </summary>
    [JsonPropertyName("length")]
    int? Length { get; }

    /// <summary>
    /// The logical size of the property value. This currently only applies to strings and is optional.
    /// </summary>
    [JsonPropertyName("size")]
    int? Size { get; }

    /// <summary>
    /// Identifies the glossary used to decode the raw property value, such as translating a map ID into a human 
    /// readable map name. 
    /// </summary>
    /// <remarks>
    /// The glossary is also used to encode and decode strings between the game internal format and UTF-16. 
    /// If a string type property has no specified charactermap, then Reference defaults to  "defaultCharacterMap". <br/>
    /// See also <see cref="IPokeAByteMapper.References"/>
    /// </remarks>
    [JsonPropertyName("reference")]
    string? Reference { get; }

    [JsonPropertyName("bits")]
    string? Bits { get; }

    /// <summary>
    /// The property description as defined in the mapper XML file.
    /// </summary>
    [JsonPropertyName("description")]
    string? Description { get; }

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    object? Value { get; set; }

    [JsonIgnore]
    object? FullValue { get; set; }

    /// <summary>
    /// The raw memory bytes from game memory as specified by the <see cref="Address"/> and <see cref="Length"/>.
    /// </summary>
    [JsonPropertyName("bytes")]
    [JsonConverter(typeof(ByteArrayJsonConverter))]
    byte[]? Bytes { get; }

    /// <summary>
    /// For frozen properties: Which bytes to write back into the game memory when changes are detected. See also
    /// <see cref="IsFrozen"/>.
    /// </summary>
    [JsonIgnore]
    byte[]? BytesFrozen { get; }

    /// <summary>
    /// Whether the property value is frozen. Whenever PokeAByte detects a change in <see cref="Bytes"/> from the 
    /// emulator, it immediately instructs the emulator to write the <see cref="FrozenBytes"/> back to the game memory
    /// at the properties <see cref="Address"/>.
    /// </summary>
    /// <remarks>
    /// This is a derived property and will be true if <see cref="BytesFrozen"/> is not null. 
    /// </remarks>
    [JsonPropertyName("isFrozen")]
    bool IsFrozen { get; }

    /// <summary>
    /// Whether a property is read only. This is true for all properties without an <see cref="Address"/>
    /// and that instead have their values populated by the respective mappers JavaScript.
    /// </summary>
    [JsonPropertyName("isReadOnly")]
    bool IsReadOnly { get; }

    /// <summary>
    /// Which property fields have been changed since the last time PokeAByte processed the property. <br/>
    /// This also applies to property changes caused by the mapper JavaScript.
    /// </summary>
    [JsonPropertyName("fieldsChanged")]
    HashSet<string> FieldsChanged { get; }

    void ProcessLoop(IPokeAByteInstance instance, IMemoryManager container, bool reloadAddresses);
    byte[] BytesFromBits(byte[] bytes);
    object? CalculateObjectValue(IPokeAByteInstance instance, byte[] bytes);
    //Exposing protected methods so we can maintian consistency
    byte[] BytesFromValue(string value, IPokeAByteMapper mapper);
    byte[] BytesFromFullValue(IPokeAByteMapper mapper);
}
