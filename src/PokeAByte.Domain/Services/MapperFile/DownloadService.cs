using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;

public record DownloadSettings
{
    [JsonPropertyName("accept")]
    public string Accept { get; set; } = "application/vnd.github.v3.raw";//"application/vnd.github+json";
    [JsonPropertyName("api_version")]
    public string ApiVersion { get; set; } = "2022-11-28";
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

public class DownloadService : IDownloadService
{
    private static string GithubSettingsFile =
        Path.Combine(BuildEnvironment.ConfigurationDirectory,
        "github_api_settings.json"
    );

    private static string LastFetchPath = Path.Combine(MapperService.MapperDirectory, "last_fetch.json");
    public DownloadSettings Settings { get; set; }
    private GithubUpdate? _lastFetch
    {
        get => field;
        set
        {
            field = value;
            JsonFile.Write(value, LastFetchPath, DomainJson.Default.GithubUpdate);
        }
    }
    private IClientNotifier _clientNotifier;
    private HttpClient _httpClient;
    private ILogger _logger;

    public DownloadService(AppSettings appSettings, ILogger<DownloadService> logger, IClientNotifier clientNotifier, HttpClient client)
    {
        _httpClient = client;
        _logger = logger;
        _clientNotifier = clientNotifier;
        Settings = JsonFile.Read(GithubSettingsFile, DomainJson.Default.DownloadSettings)
            ?? new() { Token = appSettings.GITHUB_TOKEN };
        _lastFetch = JsonFile.Read(LastFetchPath, DomainJson.Default.GithubUpdate);
    }

    private string GetGitHubUrl()
        => $"https://api.github.com/repos/{Settings.Owner}/{Settings.Repo}";
    private string GetCdnUrl(string commit)
        => $"https://cdn.jsdelivr.net/gh/{Settings.Owner}/{Settings.Repo}@{commit}";

    private async Task<HttpContent?> FetchCdnFileAsync(string path)
    {
        if (_lastFetch == null)
        {
            return null;
        }
        string url = $"{GetCdnUrl(_lastFetch.Hash)}/{path}";
        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return response.Content;
        }
        else
        {
            _logger.LogWarning($"Downloading {url} failed with HTTP status {response.StatusCode}");
        }
        return null;
    }

    public async Task<List<MapperFile>?> FetchMapperTree()
    {
        try
        {
            _logger.LogInformation("Downloading mapper_tree.json from CDN.");
            var response = await FetchCdnFileAsync("mapper_tree.json");
            if (response != null)
            {
                var data = await response.ReadFromJsonAsync(DomainJson.Default.ListMapperFile);
                return data;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception while trying to download mapper_tree.json from CDN.");
        }
        return null;
    }

    public GithubUpdate? GetLatestUpdate(bool force = false)
    {
        if (!force && _lastFetch != null && _lastFetch.LastTry + TimeSpan.FromHours(1) > DateTimeOffset.UtcNow)
        {
            return _lastFetch;
        }
        var clientRequest = new HttpRequestMessage(HttpMethod.Get, new Uri($"{GetGitHubUrl()}/commits/main"));
        try
        {
            AddGithubHeaders(clientRequest);
            var response = _httpClient.Send(clientRequest);
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize(response.Content.ReadAsStream(), DomainJson.Default.GithubCommit);
                if (data != null)
                {
                    _lastFetch = new GithubUpdate(data.Hash, DateTimeOffset.UtcNow);
                }
            }
            else
            {
                _logger.LogError($"Error requesting url {clientRequest.RequestUri}. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception encountered while requesting url {clientRequest.RequestUri}");
        }
        return _lastFetch;
    }

    private void AddGithubHeaders(HttpRequestMessage clientRequest)
    {
        clientRequest.Headers.Add("User-Agent", "Poke-A-Byte");
        clientRequest.Headers.Add("Accept", "application/vnd.github+json");
        var authorization = Settings.GetFormattedToken();
        if (authorization != null)
        {
            clientRequest.Headers.Add("Authorization", authorization);
        }
    }

    public bool ChangesAvailable()
    {
        var previousUpdate = _lastFetch;
        return GetLatestUpdate()?.Hash != previousUpdate?.Hash;
    }

    public async Task<MapperDownloadDto?> DownloadMapperAsync(string path)
    {
        try
        {
            //Get the .xml and .js paths
            var xmlPath = path;
            var jsPath = path[..path.IndexOf(".xml", StringComparison.Ordinal)] + ".js";
            //Get the xml data
            var xmlResponse = await this.FetchCdnFileAsync(xmlPath);
            var xmlData = xmlResponse != null
                ? await xmlResponse.ReadAsStringAsync()
                : null;
            //Get the js data
            var jsResponse = await this.FetchCdnFileAsync(jsPath);
            var jsData = jsResponse != null
                ? await jsResponse.ReadAsStringAsync()
                : null;
            //Add them to the list
            return new MapperDownloadDto(
                xmlPath,
                xmlData ?? throw new Exception("Missing XML. CDN returned no data for " + xmlData),
                jsPath,
                jsData ?? ""
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download of one or more mappers failed.");
            await _clientNotifier.SendError(new MapperProblem("Error", "Download of or more mappers from GitHub failed.\n" + ex.Message));
        }
        return null;
    }

    public async Task<bool> TestSettings()
    {
        var clientRequest = new HttpRequestMessage(HttpMethod.Get, $"{GetGitHubUrl()}/contents/{MapperService.MapperTreeFilename}");
        AddGithubHeaders(clientRequest);
        var authToken = Settings.GetFormattedToken();
        if (!string.IsNullOrEmpty(authToken))
        {
            clientRequest.Headers.Add("Authorization", authToken);
        }
        var result = await _httpClient.SendAsync(clientRequest);
        if (result is null)
            return false;
        return result.IsSuccessStatusCode;
    }

    public void UpdateApiSettings(DownloadSettings settings)
    {
        Settings = settings;
        JsonFile.Write(settings, GithubSettingsFile, DomainJson.Default.DownloadSettings);
    }
}
