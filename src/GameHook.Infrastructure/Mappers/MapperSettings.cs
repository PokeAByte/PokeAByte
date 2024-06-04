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
    [JsonPropertyName("requires_update")] 
    public bool RequiresUpdate { get; set; } = false;

    public static MapperSettings Load(ILogger logger)
    {
        //Setting file does not exist, just continue like normal
        if (!File.Exists(BuildEnvironment.MapperUpdateSettingsFile))
        {
            logger.LogWarning($"{BuildEnvironment.MapperUpdateSettingsFile} does not exist. " +
                              $"Mapper update settings failed to load.");
            return new MapperSettings();
        }

        //Load the json
        var jsonData = File.ReadAllText(BuildEnvironment.MapperUpdateSettingsFile);
        //Blank json data, just return 
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            logger.LogWarning($"Failed to read data from {BuildEnvironment.MapperUpdateSettingsFile}. " +
                              $"Mapper update settings failed to load.");
            return new MapperSettings();
        }

        try
        {
            //Deserialize the data 
            return JsonSerializer
                .Deserialize<MapperSettings>(jsonData) ?? new MapperSettings();
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Failed to parse {BuildEnvironment.MapperUpdateSettingsFile}. " +
                              $"Mapper update settings failed to load.");
            return new MapperSettings();
        }        
    }
    public void SaveChanges(ILogger logger)
    {
        var jsonData = JsonSerializer.Serialize(this);
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            logger.LogError("Failed to save changes to the mapper settings file.");
            return;
        }

        try
        {
            File.WriteAllText(BuildEnvironment.MapperUpdateSettingsFile,jsonData);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save changes to the mapper settings file.");
        }
    }
}
