namespace GameHook.Domain.Models.Properties;

public record UpdatePropertyFreezeModel
{
    public string Path { get; init; } = string.Empty;
    public bool Freeze { get; init; }
}