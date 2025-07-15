using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
    public AppSettings(IConfiguration configuration, ILogger<AppSettings> logger)
    {
        RETROARCH_LISTEN_IP_ADDRESS = configuration.GetRequiredValue("RETROARCH_LISTEN_IP_ADDRESS");
        RETROARCH_LISTEN_PORT = int.Parse(configuration.GetRequiredValue("RETROARCH_LISTEN_PORT"));
        RETROARCH_READ_PACKET_TIMEOUT_MS = int.Parse(configuration.GetRequiredValue("RETROARCH_READ_PACKET_TIMEOUT_MS"));
        RETROARCH_DELAY_MS_BETWEEN_READS = int.Parse(configuration.GetRequiredValue("RETROARCH_DELAY_MS_BETWEEN_READS"));

        RETROARCH_DELAY_MS_BETWEEN_READS = int.Parse(configuration.GetRequiredValue("RETROARCH_DELAY_MS_BETWEEN_READS"));

        BIZHAWK_DELAY_MS_BETWEEN_READS = int.Parse(configuration.GetRequiredValue("BIZHAWK_DELAY_MS_BETWEEN_READS"));

        PROTOCOL_FRAMESKIP = int.Parse(configuration["PROTOCOL_FRAMESKIP"] ?? "-1");

        MAPPER_VERSION = configuration.GetRequiredValue("MAPPER_VERSION");

        GITHUB_TOKEN = configuration["GITHUB_TOKEN"] ?? "";

        logger.LogInformation($"AppSettings initialized: RETROARCH_DELAY_MS_BETWEEN_READS: {RETROARCH_DELAY_MS_BETWEEN_READS} and BIZHAWK_DELAY_MS_BETWEEN_READS: {BIZHAWK_DELAY_MS_BETWEEN_READS}");
    }

    public string RETROARCH_LISTEN_IP_ADDRESS { get; }
    public int RETROARCH_LISTEN_PORT { get; }
    public int RETROARCH_READ_PACKET_TIMEOUT_MS { get; }
    public int RETROARCH_DELAY_MS_BETWEEN_READS { get; }

    public int BIZHAWK_DELAY_MS_BETWEEN_READS { get; }

    public int PROTOCOL_FRAMESKIP { get; }

    public string MAPPER_VERSION { get; }
    public string GITHUB_TOKEN { get; }
}
