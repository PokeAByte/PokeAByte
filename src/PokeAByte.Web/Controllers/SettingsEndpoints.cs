using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Web;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this WebApplication app)
    {
        app.MapPost("/settings/save_appsettings", SaveAppSettings);
        app.MapGet("/settings/appsettings", (AppSettings settings) => new AppSettingsDto
        {
            RETROARCH_LISTEN_IP_ADDRESS = settings.RETROARCH_LISTEN_IP_ADDRESS,
            RETROARCH_LISTEN_PORT = settings.RETROARCH_LISTEN_PORT,
            RETROARCH_READ_PACKET_TIMEOUT_MS = settings.RETROARCH_READ_PACKET_TIMEOUT_MS,
            DELAY_MS_BETWEEN_READS = settings.DELAY_MS_BETWEEN_READS,
            PROTOCOL_FRAMESKIP = settings.PROTOCOL_FRAMESKIP,
        });
    }

    public static IResult SaveAppSettings(
        [FromServices] IGithubService githubService,
        [FromServices] AppSettings settings,
        [FromBody] AppSettingsDto newSettings)
    {
        settings.RETROARCH_LISTEN_IP_ADDRESS = newSettings.RETROARCH_LISTEN_IP_ADDRESS;
        settings.RETROARCH_LISTEN_PORT = newSettings.RETROARCH_LISTEN_PORT;
        settings.RETROARCH_READ_PACKET_TIMEOUT_MS = newSettings.RETROARCH_READ_PACKET_TIMEOUT_MS;
        settings.DELAY_MS_BETWEEN_READS = newSettings.DELAY_MS_BETWEEN_READS;
        settings.PROTOCOL_FRAMESKIP = newSettings.PROTOCOL_FRAMESKIP;
        settings.Save();
        return TypedResults.Ok();
    }
}

public class AppSettingsDto
{
    [JsonPropertyName("RETROARCH_LISTEN_IP_ADDRESS")]
    public string RETROARCH_LISTEN_IP_ADDRESS { get; set; } = "127.0.0.1";

    [JsonPropertyName("RETROARCH_LISTEN_PORT")]
    public int RETROARCH_LISTEN_PORT { get; set; }

    [JsonPropertyName("RETROARCH_READ_PACKET_TIMEOUT_MS")]
    public int RETROARCH_READ_PACKET_TIMEOUT_MS { get; set; }

    [JsonPropertyName("DELAY_MS_BETWEEN_READS")]
    public int DELAY_MS_BETWEEN_READS { get; set; }

    [JsonPropertyName("PROTOCOL_FRAMESKIP")]
    public int PROTOCOL_FRAMESKIP { get; set; }
}