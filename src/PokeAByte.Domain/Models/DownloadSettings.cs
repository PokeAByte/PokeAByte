using System.Text.Json.Serialization;

public record DownloadSettings
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = "";

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = "PokeAByte";

    [JsonPropertyName("repo")]
    public string Repo { get; set; } = "mappers";

    [JsonPropertyName("dir")]
    public string Directory { get; set; } = "";

    public string? GetFormattedToken() => !string.IsNullOrWhiteSpace(Token)
        ? $"Bearer {Token}"
        : null;

    public string GetGithubUrl() => $"https://github.com/{Owner}/{Repo}/{Directory}";
}
