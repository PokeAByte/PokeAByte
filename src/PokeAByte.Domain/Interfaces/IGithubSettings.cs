using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Interfaces;

public interface IGithubSettings
{
    [JsonPropertyName("accept")]
    string Accept { get; set; }

    [JsonPropertyName("api_version")]
    string ApiVersion { get; set; }

    [JsonPropertyName("token")]
    string Token { get; set; }

    [JsonPropertyName("owner")]
    string Owner { get; set; }

    [JsonPropertyName("repo")]
    string Repo { get; set; }

    [JsonPropertyName("dir")]
    string Directory { get; set; }
    
    void CopySettings(IGithubSettings settings);
    string GetBaseRequestString();
    string GetFormattedToken();
    string GetGithubUrl();
}