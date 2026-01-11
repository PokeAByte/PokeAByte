using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Models;

public class AppSettings
{
    [JsonPropertyName("RETROARCH_LISTEN_IP_ADDRESS")]
    public string RETROARCH_LISTEN_IP_ADDRESS { get; set; } = "127.0.0.1";

    [JsonPropertyName("RETROARCH_LISTEN_PORT")]
    public int RETROARCH_LISTEN_PORT { get; set; } = 55355;

    [JsonPropertyName("RETROARCH_READ_PACKET_TIMEOUT_MS")]
    public int RETROARCH_READ_PACKET_TIMEOUT_MS { get; set; } = 64;

    [JsonPropertyName("DELAY_MS_BETWEEN_READS")]
    public int DELAY_MS_BETWEEN_READS { get; set; } = 5;

    [JsonPropertyName("PROTOCOL_FRAMESKIP")]
    public int PROTOCOL_FRAMESKIP { get; set; } = -1;

    [JsonPropertyName("MAPPER_VERSION")]
    public string MAPPER_VERSION { get; set; } = "0.0";

    [JsonPropertyName("GITHUB_TOKEN")]
    public string GITHUB_TOKEN { get; set; } = "";
}
