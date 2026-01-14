using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Models.Mappers;

/// <summary>
/// Metadata about a mapper that is available for download in the GitHub repository.
/// </summary>
/// <param name="Version"> Locally installed version of the mapper, if any. </param>
/// <param name="RemoteVersion"> Version of the mapper that is available on GitHub. </param>
public record RemoteMapperFile(
    string DisplayName,
    string Path,
    string? Version,
    [property: JsonPropertyName("remote_version")] string? RemoteVersion 
): MapperFile(DisplayName, Path, Version);

