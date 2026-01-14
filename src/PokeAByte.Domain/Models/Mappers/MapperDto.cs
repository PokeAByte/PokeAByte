using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Models.Mappers;

/// <summary>
/// Metadata for a mapper file.
/// </summary>
/// <param name="DisplayName"> Display name of the mapper. Can be any arbitrary text. </param>
/// <param name="Path"> The path to the mapper XML file, relative to the mapper directory. </param>
/// <param name="Version"> The version of the mapper. </param>
public record MapperFile(
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("version")] string? Version
);
