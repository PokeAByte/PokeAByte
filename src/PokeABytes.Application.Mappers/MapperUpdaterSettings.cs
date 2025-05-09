using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace PokeAByte.Application.Mappers;

public record MapperUpdaterSettings
{
    [JsonPropertyName("always_ignore_updates")]
    public bool AlwaysIgnoreUpdates { get; set; }

    [JsonPropertyName("ignore_updates_until")]
    public DateTimeOffset? IgnoreUpdatesUntil { get; set; }

    [JsonPropertyName("requires_update")]
    public bool RequiresUpdate { get; set; } = false;

    public static MapperUpdaterSettings Load(ILogger logger)
    {
        // Setting file does not exist, just continue like normal
        if (!File.Exists(MapperEnvironment.MapperUpdateSettingsFile))
        {
            logger.LogWarning($"{MapperEnvironment.MapperUpdateSettingsFile} does not exist. " +
                              $"Mapper update settings failed to load.");
            return new MapperUpdaterSettings();
        }

        // Load the json
        var jsonData = File.ReadAllText(MapperEnvironment.MapperUpdateSettingsFile);
        // Blank json data, just return 
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            logger.LogWarning($"Failed to read data from {MapperEnvironment.MapperUpdateSettingsFile}. " +
                              $"Mapper update settings failed to load.");
            return new MapperUpdaterSettings();
        }

        try
        {
            // Deserialize the data 
            return JsonSerializer.Deserialize<MapperUpdaterSettings>(jsonData)
                ?? new MapperUpdaterSettings();
        }
        catch (Exception)
        {
            logger.LogWarning($"Failed to parse {MapperEnvironment.MapperUpdateSettingsFile}. " +
                              $"Mapper update settings failed to load.");
            return new MapperUpdaterSettings();
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
            File.WriteAllText(MapperEnvironment.MapperUpdateSettingsFile, jsonData);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save changes to the mapper settings file.");
        }
    }
}
