namespace PokeAByte.Domain.Models.Mappers;

public record MapperMetaModel
{
    public required Guid Id { get; init; }
    public required string GameName { get; init; }
    public required string FileId { get; init; }
    public required string GamePlatform { get; init; }
    public required string MapperReleaseVersion { get; init; }
}