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
}

/// <summary>
/// A game property. 
/// </summary>
public interface IPokeAByteProperty
{
    /// <summary>
    /// The unique path of the property.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// The datatype of the property.
    /// </summary>
    /// <remarks>
    /// Valid types are:  <br/>
    /// - binaryCodedDecimal: A binary coded decimal. <see cref="Value"/> is an <see cref="int"/>.
    /// - bitArray: A bitfield as an array. <see cref="Value"> is a <see cref="bool[]"/>. <br/>
    /// - bool: <see cref="bool"/>. <br/>
    /// - int: <see cref="int"/>. <br/>
    /// - string: <see cref="string"/>. <br/>
    /// - uint: <see cref="uint"/>. <br/>
    /// <remarks>
    string Type { get; }

    /// <summary>
    /// Identifies the <see cref="IMemoryNamespace"/> the raw property memory is stored in. <br/>
    /// If <see langword="null"/>, then the <see cref="IMemoryManager.DefaultNamespace"/> is used.
    /// </summary>
    string? MemoryContainer { get; }

    /// <summary>
    /// The calculated memory address from which the property bytes are read.
    /// </summary>
    uint? Address { get; }

    /// <summary>
    /// The raw expression string from the mapper XML from which the <see cref="Address"/> is calculated.
    /// </summary>
    string OriginalAddressString { get; }

    // TODO: I am actually unsure if this even needs to be nullable. I don't know if there is a way
    // to create a null length property.
    /// <summary>
    /// The number bytes that makes up the the property data. <br/>
    /// The mapper XML parser defaults to <c>1</c> if the length attribute is omitted.
    /// </summary>
    int? Length { get; } 

    /// <summary>
    /// The logical size of the property value. This currently only applies to strings and is optional.
    /// </summary>
    int? Size { get; } // TODO: Further clarify how Size and Length interact for strings.

    /// <summary>
    /// Identifies the glossary used to decode the raw property value, such as translating a map ID into a human readable
    /// map name.
    /// </summary>
    /// <remarks>
    /// The glossary is also used to encode and decode strings between the game internal format and UTF-16. 
    /// If a string type property has no specified charactermap, then Reference defaults to 
    /// "defaultCharacterMap". <br/>
    /// See also <see cref="IPokeAByteMapper.References"/>
    /// </remarks>
    string? Reference { get; }

    string? Bits { get; } // TODO: Explain how the "bits" work and affect value determination.

    /// <summary>
    /// The property description as defined in the mapper XML file.
    /// </summary>
    string? Description { get; }

    object? Value { get; set; } // TODO: Explain how "Value" and "FullValue" relate to one another.
    
    object? FullValue { get; set; }

    /// <summary>
    /// The raw memory bytes from game memory as specified by the <see cref="Address"/> and <see cref="Length"/>.
    /// </summary>
    byte[]? Bytes { get; }

    /// <summary>
    /// For frozen properties: Which bytes to write back into the game memory when changes are detected. See also
    /// <see cref="IsFrozen"/>.
    /// </summary>
    byte[]? BytesFrozen { get; }

    /// <summary>
    /// Whether the property value is frozen. Whenever PokeAByte detects a change in <see cref="Bytes"/> from the 
    /// emulator, it immediately instructs the emulator to write the <see cref="FrozenBytes"/> back to the game memory
    /// at the properties <see cref="Address"/>.
    /// </summary>
    /// <remarks>
    /// This is a derived property and will be true if <see cref="BytesFrozen"/> is not null. 
    /// </remarks>
    bool IsFrozen { get; }

    /// <summary>
    /// Whether a property is read only. This is true for all properties without an <see cref="Address"/>
    /// and that instead have their values populated by the respective mappers JavaScript.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Which property fields have been changed since the last time PokeAByte processed the property. <br/>
    /// This also applies to property changes caused by the mapper JavaScript.
    /// </summary>
    /// <remarks>
    /// The identifiers used in this hashset are not exactly the same as the property names. They are: <br/>
    /// - <see cref="Path"/>: Not applicable <br/>
    /// - <see cref="Type"/>: Not applicable <br/>
    /// - <see cref="MemoryContainer"/>: <c>memoryContainer</c> <br/>
    /// - <see cref="Address"/>: <c>address</c> <br/>
    /// - <see cref="OriginalAddressString"/>: Not applicable. <br/>
    /// - <see cref="Length"/>: <c>length</c> <br/>
    /// - <see cref="Size"/>: <c>size</c> <br/>
    /// - <see cref="Reference"/>: <c>reference</c> <br/>
    /// - <see cref="Bits"/>: <c>bits</c> <br/>
    /// - <see cref="Description"/>: <c>description</c> <br/>
    /// - <see cref="Value"/>: <c>value</c> <br/>
    /// - <see cref="FullValue"/>: Not applicable. <br/>
    /// - <see cref="Bytes"/>: <c>bytes</c> <br/>
    /// - <see cref="BytesFrozen"/>: <c>frozen</c> <br/>
    /// Additionally, the following field changes may be communicated for property fields not exposed via this interface:
    /// <br/>
    /// - <c>readFunction</c> <br/>
    /// - <c>writeFunction</c> <br/>
    /// - <c>afterReadValueExpression</c> <br/>
    /// - <c>afterReadValueFunction</c> <br/>
    /// - <c>beforeWriteValueFunction</c> <br/>
    /// See <see cref="PropertyAttributes"/> for more information about those.
    /// </remarks>
    HashSet<string> FieldsChanged { get; }

    void ProcessLoop(IMemoryManager container, bool reloadAddresses);
    byte[] BytesFromBits(byte[] bytes);
    object? CalculateObjectValue(byte[] bytes);
    Task WriteValue(string value, bool? freeze);
    Task WriteBytes(byte[] bytes, bool? freeze);
    Task FreezeProperty(byte[] bytesFrozen);
    Task UnfreezeProperty();
    //Exposing protected methods so we can maintian consistency
    object? ObjectFromBytes(byte[] value);
    byte[] BytesFromValue(string value);
    byte[] BytesFromFullValue();
}
