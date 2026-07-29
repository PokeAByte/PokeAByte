namespace PokeAByte.Web.Models;

public record SetPropertyFrozenRequest
{
    /// <summary>
    /// The path of the property.
    /// </summary>
    public string Path { get; init; } = string.Empty;
    /// <summary>
    /// If true, freezes the property. If false, removes an existing freeze.
    /// </summary>
    public bool Freeze { get; init; }
}