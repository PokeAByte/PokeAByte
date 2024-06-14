namespace GameHook.Domain.Models.Mappers;

public record UpdatePropertyFreezeModel
{
    public string Path { get; init; } = string.Empty;
    public bool Freeze { get; init; }
}