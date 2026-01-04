using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Models.Mappers;

namespace PokeAByte.Domain.Models;

static class AppSettingsHelper
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        var value = configuration[key] ?? throw new Exception($"Configuration '{key}' is missing from appsettings.json");
        if (string.IsNullOrWhiteSpace(value)) throw new Exception($"Configuration '{key}' is empty.");

        return value;
    }
}

public class AppSettings
{
    public AppSettings()
    {
        RETROARCH_LISTEN_IP_ADDRESS = "127.0.0.1";
        RETROARCH_LISTEN_PORT = 55355;
        RETROARCH_READ_PACKET_TIMEOUT_MS = 64;
        DELAY_MS_BETWEEN_READS = 5;
        PROTOCOL_FRAMESKIP = -1;
        MAPPER_VERSION = "0.0";
        GITHUB_TOKEN = "";
    }

    public AppSettings(IConfiguration configuration, ILogger<AppSettings> logger)
    {
        RETROARCH_LISTEN_IP_ADDRESS = configuration.GetRequiredValue("RETROARCH_LISTEN_IP_ADDRESS");
        RETROARCH_LISTEN_PORT = int.Parse(configuration.GetRequiredValue("RETROARCH_LISTEN_PORT"));
        RETROARCH_READ_PACKET_TIMEOUT_MS = int.Parse(configuration.GetRequiredValue("RETROARCH_READ_PACKET_TIMEOUT_MS"));
        DELAY_MS_BETWEEN_READS = int.Parse(configuration.GetRequiredValue("DELAY_MS_BETWEEN_READS"));

        PROTOCOL_FRAMESKIP = int.Parse(configuration["PROTOCOL_FRAMESKIP"] ?? "-1");

        MAPPER_VERSION = configuration.GetRequiredValue("MAPPER_VERSION");

        GITHUB_TOKEN = configuration["GITHUB_TOKEN"] ?? "";

        logger.LogInformation($"AppSettings initialized: DELAY_MS_BETWEEN_READS: {DELAY_MS_BETWEEN_READS}.");
    }

    [JsonPropertyName("RETROARCH_LISTEN_IP_ADDRESS")]
    public string RETROARCH_LISTEN_IP_ADDRESS { get; set; }

    [JsonPropertyName("RETROARCH_LISTEN_PORT")]
    public int RETROARCH_LISTEN_PORT { get; set; }

    [JsonPropertyName("RETROARCH_READ_PACKET_TIMEOUT_MS")]
    public int RETROARCH_READ_PACKET_TIMEOUT_MS { get; set; }

    [JsonPropertyName("DELAY_MS_BETWEEN_READS")]
    public int DELAY_MS_BETWEEN_READS { get; set; }

    [JsonPropertyName("PROTOCOL_FRAMESKIP")]
    public int PROTOCOL_FRAMESKIP { get; set; }

    [JsonPropertyName("MAPPER_VERSION")]
    public string MAPPER_VERSION { get; set; }

    [JsonPropertyName("GITHUB_TOKEN")]
    public string GITHUB_TOKEN { get; set; }    

    public string Urls => "http://localhost:8085";
    public string AllowedHosts => "localhost";

    public void Save()
    {
        var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        var settingsText = JsonSerializer.Serialize(
            this, 
            new JsonSerializerOptions()
            {
                WriteIndented = true,
                TypeInfoResolver = MapperDtoContext.Default
            }
        );
        File.WriteAllText(
            appSettingsPath, 
            settingsText
        );
    }
}
