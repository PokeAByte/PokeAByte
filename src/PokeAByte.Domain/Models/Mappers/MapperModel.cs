using PokeAByte.Domain.Models.Properties;

namespace PokeAByte.Domain.Models.Mappers;

public record MapperModel
{
    public MapperMetaModel Meta { get; init; } = null!;
    public IEnumerable<PropertyModel> Properties { get; init; } = null!;
    public Dictionary<string, IEnumerable<GlossaryItemModel>> Glossary { get; init; } = null!;
}