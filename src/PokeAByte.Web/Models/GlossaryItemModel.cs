namespace PokeAByte.Web.Models;

/// <summary>
/// The key and value of a reference.
/// </summary>
/// <param name="Key"> The key identifiying the glossary item for resolution. </param>
/// <param name="Value"> The resolved value of the glossary item. </param>
public record GlossaryItemModel(ulong Key, object? Value);