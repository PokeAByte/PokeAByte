using System.Text.Json.Serialization;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Models.Mappers;

public record InstalledMapper(
    string DisplayName,
    string Path,
    string? Version,
    [property: JsonPropertyName("type")] MapperFileType Type 
) : MapperFile(DisplayName, Path, Version);
