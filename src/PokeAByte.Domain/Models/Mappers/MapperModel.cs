using PokeAByte.Domain.Models.Properties;

namespace PokeAByte.Domain.Models.Mappers;

public record MapperModel
{
    public required MapperMetaModel Meta { get; init; }
    public required IEnumerable<PropertyModel> Properties { get; init; }
    public required Dictionary<string, IEnumerable<GlossaryItemModel>> Glossary { get; init; }
}