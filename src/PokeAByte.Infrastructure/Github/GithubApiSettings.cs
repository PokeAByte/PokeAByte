using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PokeAByte.Application.Mappers;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Infrastructure.Github;

public record GithubApiSettings : IGithubApiSettings
{
    //Accept
    [JsonPropertyName("accept")] 
    public string Accept { get; set; } = "application/vnd.github.v3.raw";//"application/vnd.github+json";
    
    //X-GitHub-Api-Version
    [JsonPropertyName("api_version")]
    public string ApiVersion { get; set; } = "2022-11-28";
    
    //Authorization
    [JsonPropertyName("token")]
    public string Token { get; set; } = "";
    [JsonPropertyName("owner")]
    public string Owner { get; set; } = "PokeAByte";
    
    [JsonPropertyName("repo")]
    public string Repo { get; set; } = "mappers";
    
    [JsonPropertyName("dir")] 
    public string Directory { get; set; } = "";

    [JsonIgnore] public static string GithubApiUrl = "https://api.github.com/repos/";

    [JsonIgnore] public static string GithubUrl = "https://github.com";
    //[JsonIgnore] public static string GithubApiUrl = "https://raw.githubusercontent.com/";
    [JsonIgnore] private ILogger? _logger;
    public string GetAcceptValue() => Accept;

    public string GetApiVersionValue() => ApiVersion;

    public string GetTokenValue() => Token;

    public string GetFormattedToken() =>
        !string.IsNullOrWhiteSpace(Token) ? $"Bearer {Token}" : "";

    public string GetDirectory() => Directory;
    public string GetGithubUrl() => $"{GithubUrl}/{Owner}/{Repo}/{Directory}";
    protected void SetLogger(ILogger<GithubApiSettings> logger)
    {
        _logger = logger;
    }

    protected GithubApiSettings(ILogger<GithubApiSettings> logger, string? token = null)
    {
        Token = token ?? "";
        _logger = logger;
    }

    public GithubApiSettings()
    {
    }

    public string GetBaseRequestString()
    {
        if (string.IsNullOrWhiteSpace(Owner) ||
            string.IsNullOrWhiteSpace(Repo))
        {
            _logger?.LogError(
                "Cannot generate a request string because either Owner or Repo are null. \n" +
                $"Owner: {Owner}, Repo: {Repo}");
            return "";
        }

        return $"{GithubApiUrl}{Owner}/{Repo}";
    }

    public void CopySettings(IGithubApiSettings settings)
    {
        if (settings is not GithubApiSettings apiSettings)
            throw new InvalidCastException($"Failed to cast {settings.GetType()} to " +
                                           $"{typeof(GithubApiSettings)}");
        Token = apiSettings.Token;
        Owner = apiSettings.Owner;
        Repo = apiSettings.Repo;
        Directory = apiSettings.Directory;
    }
    public static GithubApiSettings Load(ILogger<GithubApiSettings> logger, string? token = null)
    {
        //Setting file does not exist, just continue like normal
        if (!File.Exists(MapperEnvironment.GithubApiSettings))
        {
            logger.LogWarning($"{MapperEnvironment.GithubApiSettings} does not exist. " +
                              $"Mapper update settings failed to load.");
            return new GithubApiSettings(logger, token);
        }
        
        //Load the json
        var jsonData = File.ReadAllText(MapperEnvironment.GithubApiSettings);        
        //Blank json data, just return 
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            logger.LogWarning($"Failed to read data from {MapperEnvironment.GithubApiSettings}. " +
                              $"Github Api settings failed to load.");
            return new GithubApiSettings(logger);
        }
        try
        {
            //Deserialize the data 
            var deserialized = JsonSerializer
                .Deserialize<GithubApiSettings>(jsonData);
            if (deserialized is null)
                return new GithubApiSettings(logger);
            deserialized.SetLogger(logger);
            deserialized.Token = string.IsNullOrWhiteSpace(deserialized.Token) ? 
                token ?? "" : 
                deserialized.Token;
            return deserialized;
        }
        catch (Exception)
        {
            logger.LogWarning($"Failed to parse {MapperEnvironment.GithubApiSettings}. " +
                              $"Github Api settings failed to load.");
            return new GithubApiSettings(logger);
        }
    }

    public void SaveChanges()
    {
        var jsonData = JsonSerializer.Serialize(this);
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            _logger?.LogError("Failed to save changes to the Github Api settings file.");
            return;
        }

        try
        {
            File.WriteAllText(MapperEnvironment.GithubApiSettings,jsonData);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Failed to save changes to the Github Api settings file.");
        }
    }

    public void Clear()
    {
        Token = "";
        Owner = "";
        Repo = "";
        Directory = "";
    }
}