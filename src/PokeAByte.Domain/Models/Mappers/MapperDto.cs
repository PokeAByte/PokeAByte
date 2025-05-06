using System.Globalization;
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

    public bool Search(string searchTerm)
    {
        return DisplayName.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase) ||
               Path.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase) ||
               Version.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase) ||
               DateCreatedUtc.ToString(CultureInfo.InvariantCulture)
                   .Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase) ||
               (DateUpdatedUtc is not null && DateUpdatedUtc.Value.ToString(CultureInfo.InvariantCulture)
                   .Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase));
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
    public string GetVersion() =>
        (string.IsNullOrEmpty(LatestVersion?.Version) ?
            string.IsNullOrEmpty(CurrentVersion?.Version) ?
                string.Empty :
                CurrentVersion?.Version :
        LatestVersion.Version) ??
        string.Empty;
    public string GetPath() =>
        (string.IsNullOrEmpty(CurrentVersion?.Path) ?
            string.IsNullOrEmpty(LatestVersion?.Path) ?
                string.Empty :
                LatestVersion?.Path :
            CurrentVersion?.Path) ??
        string.Empty;

    public bool Search(string searchFilter)
    {
        if (LatestVersion is null && CurrentVersion is null)
            return false;
        if (string.IsNullOrEmpty(searchFilter))
            return true;
        if (LatestVersion is not null && LatestVersion.Search(searchFilter))
            return true;
        if (CurrentVersion is not null && CurrentVersion.Search(searchFilter))
            return true;
        return false;
    }
}

