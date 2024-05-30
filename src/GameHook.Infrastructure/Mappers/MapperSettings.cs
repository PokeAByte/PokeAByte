using System.Text.Json;
using System.Text.Json.Serialization;
using GameHook.Domain;
using Microsoft.Extensions.Logging;

namespace GameHook.Infrastructure.Mappers;

public record MapperSettings
{
    [JsonPropertyName("always_ignore_updates")]
    public bool AlwaysIgnoreUpdates { get; set; }
    [JsonPropertyName("ignore_updates_until")]
    public DateTimeOffset? IgnoreUpdatesUntil { get; set; }
    [JsonPropertyName("mapper_download_base_url")]
    public string? MapperDownloadBaseUrl { get; set; }
    [JsonPropertyName("archive_limit")]
    public int ArchiveLimit { get; set; } = 10;
    [JsonPropertyName("mappers")] 
    public List<Mapper> Mappers { get; set; } = [];
}

public record Mapper
{
    [JsonPropertyName("mapper_display_name")]
    public string DisplayName { get; set; } = "";
    [JsonPropertyName("mapper_filename")]
    public string Filename { get; set; } = "";
    [JsonPropertyName("mapper_current_version")]
    public string CurrentVersion { get; set; } = "";
    [JsonPropertyName("mapper_latest_version")]
    public string LatestVersion { get; set; } = "";
}
public class MapperSettingsManager
{
    public MapperSettings MapperSettings = new();
    private readonly ILogger<MapperSettings> _logger;
    public MapperSettingsManager(ILogger<MapperSettings> logger)
    {
        _logger = logger;
        //Setting file does not exist, just continue like normal
        if (!File.Exists(BuildEnvironment.MapperUpdateSettingsFile))
        {
            logger.LogWarning($"{BuildEnvironment.MapperUpdateSettingsFile} does not exist. " +
                              $"Mapper update settings failed to load.");
            return;
        }

        //Load the json
        var jsonData = File.ReadAllText(BuildEnvironment.MapperUpdateSettingsFile);
        //Blank json data, just return 
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            logger.LogWarning($"Failed to read data from {BuildEnvironment.MapperUpdateSettingsFile}. " +
                              $"Mapper update settings failed to load.");
           return; 
        }

        try
        {
            //Deserialize the data 
            MapperSettings = JsonSerializer
                .Deserialize<MapperSettings>(jsonData) ?? new MapperSettings();
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Failed to parse {BuildEnvironment.MapperUpdateSettingsFile}. " +
                              $"Mapper update settings failed to load.");
        }
    }

    public void SaveChanges()
    {
        var jsonData = JsonSerializer.Serialize(MapperSettings);
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            _logger.LogError("Failed to save changes to the mapper settings file.");
            return;
        }

        try
        {
            File.WriteAllText(BuildEnvironment.MapperUpdateSettingsFile,jsonData);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save changes to the mapper settings file.");
        }
    }
}