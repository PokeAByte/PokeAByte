using PokeAByte.Domain;

namespace PokeAByte.Web.Models;

public record MapperArchiveModel
{
    public required string BasePath { get; init; }
    public List<ArchivedMapperDto> MapperModels { get; init; }
    //public required Dictionary<string, List<ArchivedMapperDto>> MapperList { get; init; }
    public bool IsExpanded { get; set; } = false;
    public bool IsSelected { get; set; } = false;
    public bool IsChangingName { get; set; } = false;
    public required string DisplayName { get; set; }
    public required string Hash { get; set; }
    public ArchiveType Type { get; set; }
}

public enum ArchiveType
{
    None,
    Archived,
    BackUp
}
public record MapperArchiveDto
{
    public required string BasePath { get; init; }
    public required string DisplayName { get; set; }
    public required string Hash { get; set; }
    public required ArchiveType Type { get; set; }
}