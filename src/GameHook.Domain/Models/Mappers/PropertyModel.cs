using System.Text.Json.Serialization;

namespace GameHook.Domain.Models.Mappers;

public class PropertyModel
{
    public string Path { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string? MemoryContainer { get; init; } = string.Empty;

    public uint? Address { get; init; }

    public int? Length { get; init; }

    public int? Size { get; init; }

    public string? Reference { get; init; }

    public string? Bits { get; init; }

    public string? Description { get; init; }


    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public object? Value { get; init; }

    public IEnumerable<int>? Bytes { get; init; } = Enumerable.Empty<int>();

    public bool? IsFrozen { get; init; }

    public bool IsReadOnly { get; init; }
}