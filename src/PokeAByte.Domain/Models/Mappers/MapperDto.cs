using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Models.Mappers;

public record MapperDto
{
    [JsonPropertyName("display_name")] public required string DisplayName { get; init; }
    [JsonPropertyName("path")] public required string Path { get; init; }
    [JsonPropertyName("date_created")] public DateTime DateCreatedUtc { get; init; }
    [JsonPropertyName("date_updated")] public DateTime? DateUpdatedUtc { get; init; }
    [JsonPropertyName("version")] public string Version { get; init; } = "";

    public static MapperDto Create(string baseDirectory, string filepath, string version = "")
    {
        if (!File.Exists(filepath))
            throw new Exception($"Failed to open file {filepath}.");
        var file = new FileInfo(filepath);
        var path = filepath[baseDirectory.Length..].Replace("\\", "/");
        return new MapperDto
        {
            Path = path,
            DisplayName = file.Name,
            Version = version,
            DateCreatedUtc = file.CreationTimeUtc,
            DateUpdatedUtc = file.LastWriteTimeUtc <= file.CreationTimeUtc ?
                null :
                file.LastWriteTimeUtc
        };
    }

    public bool Outdated(MapperDto? latest)
    {
        if (latest is null)
            return false;
        if (latest.Path != Path)
            throw new InvalidOperationException($"{DisplayName} is not {latest.DisplayName}, cannot compare.");
        if (latest.DateUpdatedUtc is null)
            return false;
        if (latest.DateUpdatedUtc is not null && DateUpdatedUtc is null)
            return true;
        if (latest.DateUpdatedUtc is not null && DateUpdatedUtc is not null && latest.DateUpdatedUtc > DateUpdatedUtc)
            return true;
        return false;
    }
}

[JsonSerializable(typeof(List<MapperDto>))]
public partial class MapperDtoContext : JsonSerializerContext
{
}

public record MapperComparisonDto
{
    public MapperDto? CurrentVersion { get; set; } = null;
    public MapperDto? LatestVersion { get; set; } = null;
}
