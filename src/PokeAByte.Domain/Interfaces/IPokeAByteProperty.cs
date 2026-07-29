using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Interfaces;

/// <summary>
/// The game property data that clients receive and can interact with.
/// </summary>
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
    int Length { get; }

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

    /// <summary>
    /// Describes which bits are being read from the game memory to derive the value. <br/>
    /// </summary>
    /// <value>
    /// One of the following formats: <br/>
    /// Single bit: `"y"` (e.g. `"0"`) <br/>
    /// Range: `"x-y"` (e.g. `"0-3"`) <br/>
    /// Array: `"x,y,z"` (e.g. `"0,2,4,8"`)
    /// </value>
    [JsonPropertyName("bits")]
    string? Bits { get; }

    /// <summary>
    /// The property description as defined in the mapper XML file.
    /// </summary>
    [JsonPropertyName("description")]
    string? Description { get; }

    /// <summary>
    /// The value derived from the game memory.
    /// </summary>
    /// <value>
    /// Can have be of the following .NET types depending on <see cref="Type"/>: <br/>
    /// <see cref="PropertyType.BinaryCodedDecimal"/> -> int <br/>
    /// <see cref="PropertyType.BitArray"/> -> bool[] <br/>
    /// <see cref="PropertyType.Bool"/> -> bool <br/>
    /// <see cref="PropertyType.Bit"/> -> bool <br/>
    /// <see cref="PropertyType.Int"/> -> int <br/>
    /// <see cref="PropertyType.String"/> -> string <br/>
    /// <see cref="PropertyType.Uint"/> -> uint <br/>
    /// <see cref="PropertyType.ByteArray"/> -> byte[]
    /// </value>
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    object? Value { get; set; }

    [JsonIgnore]
    object? FullValue { get; set; }

    /// <summary>
    /// The raw memory bytes from game memory as specified by the <see cref="Address"/> and <see cref="Length"/>.
    /// </summary>
    [JsonPropertyName("bytes")]
    byte[] Bytes { get; }

    /// <summary>
    /// For frozen properties: Which bytes to write back into the game memory when changes are detected. See also
    /// <see cref="IsFrozen"/>.
    /// </summary>
    [JsonIgnore]
    byte[] BytesFrozen { get; }

    /// <summary>
    /// Whether the property value is frozen. Whenever PokeAByte detects a change in <see cref="Bytes"/> from the 
    /// emulator, it immediately instructs the emulator to write the <see cref="BytesFrozen"/> back to the game memory
    /// at the properties <see cref="Address"/>. <br/>
    /// Emulators fully supporting the "emulator data protocol" may also freeze the memory internally.
    /// </summary>
    /// <remarks>
    /// This is a derived property and will be true if <see cref="BytesFrozen"/> is not empty. 
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
    [JsonConverter(typeof(FieldChangesJsonConverter))]
    FieldChanges FieldsChanged { get; internal set; }

    void ProcessLoop(IPokeAByteInstance instance, IMemoryManager container, bool reloadAddresses);
    byte[] BytesFromBits(byte[] bytes);
    object? CalculateObjectValue(IPokeAByteInstance instance, byte[] bytes);
    //Exposing protected methods so we can maintian consistency
    byte[] BytesFromValue(string value, IPokeAByteMapper mapper);
    byte[] BytesFromFullValue(IPokeAByteMapper mapper);
}
