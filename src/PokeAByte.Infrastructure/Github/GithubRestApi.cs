using System.Text.Json.Serialization;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Infrastructure.Github;

[JsonSerializable(typeof(GithubSettings))]
[JsonSerializable(typeof(IGithubSettings))]
public partial class GithubSettingsContext : JsonSerializerContext;

public record GithubSettings : IGithubSettings
{
    [JsonPropertyName("accept")]
    public string Accept { get; set; } = "application/vnd.github.v3.raw";//"application/vnd.github+json";
    [JsonPropertyName("api_version")]
    public string ApiVersion { get; set; } = "2022-11-28";
    [JsonPropertyName("token")]
    public string Token
    {
        get
        {
            return token;
        }

        set
        {
            token = value;
        }
    }
    [JsonPropertyName("owner")]
    public string Owner { get; set; } = "PokeAByte";
    [JsonPropertyName("repo")]
    public string Repo { get; set; } = "mappers";
    [JsonPropertyName("dir")]
    public string Directory { get; set; } = "";

    [JsonIgnore]
    public static string GithubApiUrl = "https://api.github.com/repos/";

    [JsonIgnore]
    public static string GithubUrl = "https://github.com";
    private string token = "";

    public string GetGithubUrl() => $"{GithubUrl}/{Owner}/{Repo}/{Directory}";

    public string GetBaseRequestString()
    {
        if (string.IsNullOrWhiteSpace(Owner) || string.IsNullOrWhiteSpace(Repo))
        {
            return "";
        }
        return $"{GithubApiUrl}{Owner}/{Repo}";
    }

    public void CopySettings(IGithubSettings settings)
    {
        if (settings is not GithubSettings apiSettings)
            throw new InvalidCastException($"Failed to cast {settings.GetType()} to {typeof(GithubSettings)}");
        Token = apiSettings.Token;
        Owner = apiSettings.Owner;
        Repo = apiSettings.Repo;
        Directory = apiSettings.Directory;
    }

    public string GetFormattedToken() => !string.IsNullOrWhiteSpace(Token) ? $"Bearer {Token}" : "";
}
