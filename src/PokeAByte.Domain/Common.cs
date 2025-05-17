namespace PokeAByte.Domain;

/// <summary>
/// Endianness enum.
/// </summary>
public enum EndianTypes : byte
{
    BigEndian,
    LittleEndian
}

/// <summary>
/// A memory block as defined by the starting and ending address.
/// </summary>
/// <param name="Name"> Name of the address block. </param>
/// <param name="StartingAddress"> The start of the block. </param>
/// <param name="EndingAddress"> The end of the block. </param>
public record MemoryAddressBlock(string Name, MemoryAddress StartingAddress, MemoryAddress EndingAddress);

/// <summary>
/// Contains the key-value pairs for a mapper reference (aka "Glossary") collection.
/// </summary>
public class ReferenceItems
{
    /// <summary>    
    /// The name of the reference collection.     
    /// </summary>    
    /// <remarks>    
    /// The name is derived from the element name in the <c>&lt;references&gt;</c> element of the mapper. E.g.    
    /// <code>    
    /// &lt;references&gt;    
    ///     &lt;save_file_status&gt; ... &lt;/save_file_status&gt;    
    /// &lt;/references&gt;    
    /// </code>    
    /// Results in the reference name "save_file_status".    
    /// </remarks>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional type hint for the values of the individual reference values. This is (currently) purely 
    /// informational for Poke-A-Byte-clients.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// The key-values for the reference.
    /// </summary>
    /// <remarks>
    /// They are parsed from the direct descendants of the reference element. E.g
    /// <code>
    /// &lt;references&gt;
    ///     &lt;save_file_status&gt; 
    ///         &lt;entry key="0x00" value="Instant" /&gt;
    ///     &lt;/save_file_status&gt;
    /// &lt;/references&gt;
    /// </code>
    /// Creates one <c>Values</c> <see cref="ReferenceItem"/> with <see cref="ReferenceItem.Key"/> <c>0x00</c> and
    /// <see cref="ReferenceItem.Value"/> <c>"Instant"</c>. <br/>
    /// While the element name <c>entry</c> is not checked for, mapper authors should adhere to the convenntion.
    /// </remarks>
    public IEnumerable<ReferenceItem> Values { get; init; } = new List<ReferenceItem>();

    /// <summary>
    /// Find a reference key-value pair by its key.
    /// </summary>
    /// <param name="key"> The key of the desired item. </param>
    /// <returns> The found item or <see langword="null"/> if no item with that key exists. </returns>
    public ReferenceItem? GetSingleOrDefaultByKey(ulong key)
    {
        return Values.SingleOrDefault(x => x.Key == key);
    }

    /// <summary>
    /// Find a reference key-value pair by its value.
    /// </summary>
    /// <param name="key"> The value of the desired item. </param>
    /// <returns> The found item or <see langword="null"/> if no item with that value exists. </returns>
    public ReferenceItem GetFirstByValue(object? value)
    {
        return Values.FirstOrDefault(x => string.Equals(x.Value?.ToString(), value?.ToString(), StringComparison.Ordinal)) ?? throw new Exception($"Missing dictionary value for '{value}', value was not found in reference list {Name}.");
    }
}

/// <summary>
/// A key-value pair for the mapper glossary / reference. 
/// </summary>
/// <param name="Key"> They key for the reference value. </param>
/// <param name="Value"> The reference value. </param>
public record ReferenceItem(ulong Key, object? Value);
