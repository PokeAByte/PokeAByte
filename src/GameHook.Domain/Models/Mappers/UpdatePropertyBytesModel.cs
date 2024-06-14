namespace GameHook.Domain.Models.Mappers;

public record UpdatePropertyBytesModel
{
    public string Path { get; init; } = string.Empty;
    public int[] Bytes { get; init; } = Array.Empty<int>();
    public bool? Freeze { get; init; }
}