namespace PokeAByte.Domain.Models.Mappers;

public record MapperFileModel
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => DisplayName;
}