namespace GameHook.Domain.Models.Properties;

public record UpdatePropertyValueModel
{
    public string Path { get; init; } = string.Empty;
    public object? Value { get; init; }
    public bool? Freeze { get; init; }
}