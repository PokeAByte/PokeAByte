using System.Text.Json;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Logic;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Services.MapperFile;

namespace PokeAByte.Infrastructure.Github;

public class GitHubService : IGithubService
{
    private readonly ILogger<IGithubService> _logger;
    private readonly IClientNotifier _clientNotifier;
    private HttpContent? _cachedTreeFileResponse = null;
    private DateTime _treeFileCacheTime = DateTime.MinValue;

    public IGithubSettings Settings { get; private set; }

    public GitHubService(ILogger<IGithubService> logger, AppSettings appSettings, IClientNotifier clientNotifier)
    {
        _logger = logger;
        _clientNotifier = clientNotifier;
        Settings = LoadSettings(appSettings.GITHUB_TOKEN)
            ?? new GithubSettings() { Token = appSettings.GITHUB_TOKEN };
    }

    private GithubSettings? LoadSettings(string defaultToken = "")
    {

        // Load the json
        string? jsonData = File.Exists(MapperPaths.GithubApiSettings)
            ? File.ReadAllText(MapperPaths.GithubApiSettings)
            : null;
        // Blank json data, just return 
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            return null;
        }
        try
        {
            //Deserialize the data 
            var deserialized = JsonSerializer.Deserialize(jsonData, GithubSettingsContext.Default.GithubSettings);
            if (deserialized is null)
                return null;

            deserialized.Token = string.IsNullOrWhiteSpace(deserialized.Token)
                ? defaultToken
                : deserialized.Token;
            return deserialized;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<string?> ResponseMessageToJson(HttpResponseMessage? responseMessage)
    {
        if (responseMessage is null || !responseMessage.IsSuccessStatusCode)
            return null;
        try
        {
            return await responseMessage.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to read message response.");
            return null;
        }
    }

    private async Task<HttpResponseMessage?> GetContentRequest(string? path = null, bool isFile = false)
    {
        var url = Settings.GetBaseRequestString();
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogError("The Url generated for the GET request was empty.");
            return null;
        }

        if (path is null && isFile is false)
        {
            _logger.LogInformation($"The path for sending a GET request is null, defaulting to {Settings.Directory}.");
            path = Settings.Directory;
        }
        else if (path is null && isFile)
        {
            return null;
        }

        if (path != string.Empty)
        {
            url += $"/contents/{path}";
        }
        var clientRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
            Headers =
            {
                {"Accept", Settings.Accept},
                {"X-GitHub-Api-Version", Settings.ApiVersion},
                {"User-Agent", "request"}
            }
        };
        var authToken = Settings.GetFormattedToken();
        if (!string.IsNullOrEmpty(authToken))
        {
            clientRequest.Headers.Add("Authorization", authToken);
        }

        using var client = new HttpClient();
        return await client.SendAsync(clientRequest);
    }

    public async Task<HttpContent?> GetMapperTreeFile()
    {
        if (_cachedTreeFileResponse == null || _treeFileCacheTime > DateTime.Now + TimeSpan.FromMinutes(1))
        {
            var response = await GetContentRequest(MapperPaths.MapperTreeJson, true);
            if (response is null || !response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to download the latest version of the mapper tree json from Github.");
                await _clientNotifier.SendError(new MapperProblem("Error", "Failed to fetch latest mapper versions from GitHub"));
                return _cachedTreeFileResponse;
            }
            _cachedTreeFileResponse = response.Content;
            _treeFileCacheTime = DateTime.Now;
        }
        return _cachedTreeFileResponse;
    }

    public async Task<List<UpdateMapperDto>> DownloadMappersAsync(List<MapperDto> mapperDtos)
    {
        var count = 0;
        List<UpdateMapperDto> downloadedMappers = new();
        try
        {            
            foreach (var mapper in mapperDtos)
            {
                //Get the .xml and .js paths
                var xmlPath = mapper.Path;
                var jsPath = mapper.Path[..mapper.Path.IndexOf(".xml", StringComparison.Ordinal)] + ".js";
                //Get the xml data
                var xmlResponse = await GetContentRequest(xmlPath, true);
                var xmlData = await ResponseMessageToJson(xmlResponse);
                //Get the js data
                var jsResponse = await GetContentRequest(jsPath, true);
                var jsData = await ResponseMessageToJson(jsResponse);
                //Add them to the list
                downloadedMappers.Add(new UpdateMapperDto(
                    xmlPath, xmlData ?? "",
                    jsPath, jsData ?? "",
                    mapper.DateCreatedUtc, mapper.DateUpdatedUtc));
                count++;
            }
        } catch(Exception ex)
        {
            await _clientNotifier.SendError(new MapperProblem("Error", "Download of or more mappers from GitHub failed.\n"+ex.Message));
        }
        return downloadedMappers;
    }

    public async Task<bool> TestSettings()
    {
        var result = await GetContentRequest(MapperPaths.MapperTreeJson, true);
        if (result is null)
            return false;
        return result.IsSuccessStatusCode;
    }

    public void ApplySettings(IGithubSettings settings)
    {
        this.Settings.CopySettings(settings);
        var jsonData = JsonSerializer.Serialize(settings, GithubSettingsContext.Default.GithubSettings);
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            _logger.LogError("Failed to save changes to the Github settings file.");
            return;
        }

        try
        {
            File.WriteAllText(MapperPaths.GithubApiSettings, jsonData);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Failed to save changes to the Github settings file.");
        }
    }
}
