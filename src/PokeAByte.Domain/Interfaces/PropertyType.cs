using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Interfaces;

/// <summary>
/// Possible datatypes that game memory can be converted to and from.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PropertyType>))]
public enum PropertyType : byte
{
    /// <summary>
    /// A binary coded decimal signed integer.
    /// </summary>
    [JsonStringEnumMemberName("binaryCodedDecimal")]
    BinaryCodedDecimal,

    /// <summary>
    /// An array of boolean values.
    /// </summary>
    [JsonStringEnumMemberName("bitArray")]
    BitArray,

    /// <summary>
    /// A boolean.
    /// </summary>
    [JsonStringEnumMemberName("bool")]
    Bool,

    /// <summary>
    /// A boolean. This datatype is deprecated and should not be used in configurations, use <see cref="Bool"/> instead.
    /// </summary>
    [JsonStringEnumMemberName("bit")]
    Bit,

    /// <summary>
    /// A signed integer.
    /// </summary>
    [JsonStringEnumMemberName("int")]
    Int,

    /// <summary>
    /// A string.
    /// </summary>
    [JsonStringEnumMemberName("string")]
    String,

    /// <summary>
    /// An unsigned integer.
    /// </summary>
    [JsonStringEnumMemberName("uint")]
    Uint,

    /// <summary>
    /// An array of bytes.
    /// </summary>
    [JsonStringEnumMemberName("byteArray")]
    ByteArray
}
