using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace PokeAByte.Application.Mappers;

public class MapperUpdaterSettings
{
    [JsonPropertyName("always_ignore_updates")]
    public bool AlwaysIgnoreUpdates { get; set; }

    [JsonPropertyName("ignore_updates_until")]
    public DateTimeOffset? IgnoreUpdatesUntil { get; set; }

    [JsonPropertyName("requires_update")]
    public bool RequiresUpdate { get; set; } = false;

    private static string? ReadFileIfExists(string path) {
        if (!File.Exists(path)) {
            return null;
        }
        string result = File.ReadAllText(path);
        if (string.IsNullOrEmpty(result)) {
            return null;
        }
        return result;
    }

    public static MapperUpdaterSettings Load(ILogger<MapperUpdaterSettings> logger)
    {
        var settingsJson = ReadFileIfExists(MapperEnvironment.MapperUpdateSettingsFile);
        MapperUpdaterSettings? result = null;
        if (settingsJson != null) {
            try
            {
                // Deserialize the data 
                result = JsonSerializer.Deserialize<MapperUpdaterSettings?>(settingsJson);       
            }
            catch (Exception) { }
        }
        if (result == null) {
            logger.LogWarning($"Failed to read {MapperEnvironment.MapperUpdateSettingsFile}. Creating a new one. ");
            result = new MapperUpdaterSettings();
            result.SaveChanges(logger);
        }
        return result;
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
