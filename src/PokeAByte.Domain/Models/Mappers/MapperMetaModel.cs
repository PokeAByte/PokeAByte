namespace PokeAByte.Domain.Models.Mappers;

public record MapperMetaModel
{
    public Guid Id { get; init; }
    public string GameName { get; init; } = string.Empty;
    public string GamePlatform { get; init; } = string.Empty;
    public string MapperReleaseVersion { get; init; } = string.Empty;
}