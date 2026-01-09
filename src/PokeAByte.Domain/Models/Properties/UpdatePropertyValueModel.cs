using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Models.Properties;

public record UpdatePropertyValueModel
{
    public string Path { get; init; } = string.Empty;
    [JsonConverter(typeof(ObjectToInferredTypesConverter))]
    public object? Value { get; init; }
    public bool? Freeze { get; init; }
}