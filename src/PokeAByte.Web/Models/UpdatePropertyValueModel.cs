using System.Text.Json.Serialization;
using PokeAByte.Web.Json;

namespace PokeAByte.Web.Models;

public record UpdatePropertyValueModel
{
    public string Path { get; init; } = string.Empty;
    [JsonConverter(typeof(ObjectToInferredTypesConverter))]
    public object? Value { get; init; }
    public bool? Freeze { get; init; }
}