using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Web.Models;

public record MapperModel
{
    public MapperMetaModel Meta { get; init; } = null!;
    public IEnumerable<IPokeAByteProperty> Properties { get; init; } = null!;
    public Dictionary<string, IEnumerable<GlossaryItemModel>> Glossary { get; init; } = null!;
}