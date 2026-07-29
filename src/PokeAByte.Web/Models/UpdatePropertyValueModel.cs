using System.Text.Json.Serialization;
using PokeAByte.Web.Json;

namespace PokeAByte.Web.Models;

public record SetPropertyValueRequest
{
    /// <summary>
    /// The path of the property
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// The new value for the target property.
    /// </summary>
    [JsonConverter(typeof(ObjectToInferredTypesConverter))]
    public object? Value { get; init; }

    /// <summary>
    /// true: freezes the property to the new value. <br/>
    /// false: Removes the freeze (if one is set). <br/>
	/// null: the freeze is not changed.
    /// </summary>
    public bool? Freeze { get; init; }
}