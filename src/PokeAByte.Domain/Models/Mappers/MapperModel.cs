using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Properties;

namespace PokeAByte.Domain.Models.Mappers;

public record MapperModel
{
    public MapperMetaModel Meta { get; init; } = null!;
    public IEnumerable<IPokeAByteProperty> Properties { get; init; } = null!;
    public Dictionary<string, IEnumerable<GlossaryItemModel>> Glossary { get; init; } = null!;
}