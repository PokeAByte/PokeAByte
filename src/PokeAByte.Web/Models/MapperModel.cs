using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Web.Models;

/// <summary>
/// Model describing the currently loaded mapper.
/// </summary>
public record MapperModel
{
    /// <summary>
    /// Mapper meta data.
    /// </summary>
    public MapperMetaModel Meta { get; init; } = null!;
    /// <summary>
    /// The properties defined by the mapper with their latest values.
    /// </summary>
    public IEnumerable<IPokeAByteProperty> Properties { get; init; } = null!;
    /// <summary>
    /// The glossary or references items defined by the mapper.
    /// </summary>
    public Dictionary<string, IEnumerable<GlossaryItemModel>> Glossary { get; init; } = null!;
}