using PokeAByte.Domain;

namespace PokeAByte.Web.Models;

public record MapperArchiveModel
{
    public required string BasePath { get; init; }
    public List<ArchivedMapperDto> MapperModels { get; init; }
    //public required Dictionary<string, List<ArchivedMapperDto>> MapperList { get; init; }
    public bool IsExpanded { get; set; } = false;
    public bool IsSelected { get; set; } = false;
}
