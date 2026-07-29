namespace PokeAByte.Web.Models;

public record SetPropertyBytesRequest
{
    /// <summary>
    /// The path of the property
    /// </summary>
    public string Path { get; init; } = string.Empty;
    
    /// <summary>
    /// The array of byte values to write into the property.
    /// </summary>
    public int[] Bytes { get; init; } = [];

    /// <summary>
    /// Whether to freeze/unfreeze the property.
    /// </summary>
    /// <value>
    /// If true, freezes the property to the new value. If false, removes the freeze (if one is set). 
	/// If null the freeze is not changed.
    /// </value>
    public bool? Freeze { get; init; }
}