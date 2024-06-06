using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameHook.Mappers;
public class MapperUpdateManager : IMapperUpdateManager
{
    private readonly ILogger<MapperUpdateManager> _logger;
    private readonly AppSettings _appSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MapperUpdaterSettings _mapperUpdaterSettings;
    private readonly IMapperArchiveManager _mapperArchiveManager;
    private readonly IGithubRestApi _githubRestApi;

    public MapperUpdateManager(ILogger<MapperUpdateManager> logger, 
        AppSettings appSettings, 
        IHttpClientFactory httpClientFactory,
        MapperUpdaterSettings mapperUpdaterSettingsManager, 
        IGithubRestApi githubRestApi, 
        IMapperArchiveManager mapperArchiveManager)
    {
        _logger = logger;
        _appSettings = appSettings;
        _httpClientFactory = httpClientFactory;
        _mapperUpdaterSettings = mapperUpdaterSettingsManager;
        _githubRestApi = githubRestApi;
        _mapperArchiveManager = mapperArchiveManager;

        if (Directory.Exists(BuildEnvironment.ConfigurationDirectory) == false)
        {
            _logger.LogInformation("Creating configuration directory for GameHook.");

            Directory.CreateDirectory(BuildEnvironment.ConfigurationDirectory);
        }
    }
    

    private static void CleanupTemporaryFiles()
    {
        // TODO: 2/8/2024 - Remove this at a future date.
        var oldMapperJsonFile = Path.Combine(BuildEnvironment.ConfigurationDirectory, "mappers.json");
        if (File.Exists(oldMapperJsonFile))
        {
            File.Delete(oldMapperJsonFile);
        }

        if (File.Exists(MapperEnvironment.MapperTemporaryZipFilepath))
        {
            File.Delete(MapperEnvironment.MapperTemporaryZipFilepath);
        }

        if (Directory.Exists(MapperEnvironment.MapperTemporaryExtractionDirectory))
        {
            Directory.Delete(MapperEnvironment.MapperTemporaryExtractionDirectory, true);
        }
    }

    private static async Task DownloadMappers(HttpClient httpClient, string distUrl)
    {
        try
        {
            CleanupTemporaryFiles();

            // Download the ZIP from Github.
            var bytes = await httpClient.GetByteArrayAsync(distUrl);
            await File.WriteAllBytesAsync(MapperEnvironment.MapperTemporaryZipFilepath, bytes);

            // Extract to the temporary directory.
            using var zout = ZipFile.OpenRead(MapperEnvironment.MapperTemporaryZipFilepath);
            zout.ExtractToDirectory(MapperEnvironment.MapperTemporaryExtractionDirectory);

            var mapperTemporaryExtractionSubfolderDirectory = Directory.GetDirectories(MapperEnvironment.MapperTemporaryExtractionDirectory).FirstOrDefault() ??
                throw new Exception("Mappers were downloaded from the server, but did not contain a subfolder.");

            if (Directory.Exists(MapperEnvironment.MapperLocalDirectory))
            {
                //make a zipped archived of the old mappers
                ZipFile.CreateFromDirectory(MapperEnvironment.MapperLocalDirectory, 
                    Path.Combine(MapperEnvironment.MapperLocalArchiveDirectory, 
                        $"Mapper_{DateTime.Now:yyyyMMddhhmm}"));
                
                Directory.Delete(MapperEnvironment.MapperLocalDirectory, true);
            }

            // Move from inside of the temporary directory into the main mapper folder.
            Directory.Move(mapperTemporaryExtractionSubfolderDirectory, MapperEnvironment.MapperLocalDirectory);
        }
        finally
        {
            CleanupTemporaryFiles();
        }
    }

    //Returns a list of outdated mappers 
    private async Task<List<MapperComparisonDto>> GetOutdatedMapperList()
    {
        //Get the latest version of the mapper tree from github
        var mapperListResponse = await _githubRestApi.GetMapperTreeFile();
        if (mapperListResponse is null || !mapperListResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to download the latest version of the mapper tree json from Github.");
            return [];
        }
        //Convert the remote data to a MapperDto
        var remote = await HttpContentJsonExtensions.ReadFromJsonAsync<List<MapperDto>>(mapperListResponse.Content);
        if (remote is null || remote.Count == 0)
        {
            _logger.LogError("Could not read the remote json file.");
            return [];
        }
        //Get the current version the user has on their filesystem
        var mapperTree = MapperTreeUtility.Load(MapperEnvironment.MapperLocalDirectory);
        //Compare the mapper trees and return a list of outdated mappers
        return mapperTree.CompareMapperTrees(remote);

    }
    public async Task<bool> CheckForUpdates()
    {
        try
        {
            if (BuildEnvironment.IsDebug && _appSettings.MAPPER_DIRECTORY_OVERWRITTEN)
            {
                _logger.LogWarning("Mapper directory is overwritten, will not perform any updates.");
                _mapperUpdaterSettings.RequiresUpdate = false;
                _mapperUpdaterSettings.SaveChanges(_logger);
                return false;
            }

            if (_mapperUpdaterSettings.AlwaysIgnoreUpdates)
            {
                _logger.LogInformation("User requested to ignore updates.");
                _mapperUpdaterSettings.RequiresUpdate = false;
                _mapperUpdaterSettings.SaveChanges(_logger);
                return false;
            }

            if (_mapperUpdaterSettings.IgnoreUpdatesUntil is not null &&
                _mapperUpdaterSettings.IgnoreUpdatesUntil > DateTime.Now)
            {
                _mapperUpdaterSettings.RequiresUpdate = false;
                _mapperUpdaterSettings.SaveChanges(_logger);
                return false;
            }
            //`IgnoreUpdatesUntil` timeframe has passed, remove the value
            _mapperUpdaterSettings.IgnoreUpdatesUntil = null;
            //Get the list of outdated mappers 
            var outdatedMappers = await GetOutdatedMapperList();
            //Save the outdated mapper list
            var jsonData = JsonSerializer.Serialize(outdatedMappers);
            await File.WriteAllTextAsync(MapperEnvironment.OutdatedMapperTreeJson,jsonData);
            _mapperUpdaterSettings.RequiresUpdate = true;
            _mapperUpdaterSettings.SaveChanges(_logger);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not perform update check for mappers.");
            return false;
        }
    }
    public async Task<bool> CheckForUpdatesDeprecated()
    {
        try
        {
            if (BuildEnvironment.IsDebug && _appSettings.MAPPER_DIRECTORY_OVERWRITTEN)
            {
                _logger.LogWarning("Mapper directory is overwritten, will not perform any updates.");
                return false;
            }

            if (string.IsNullOrEmpty(_appSettings.MAPPER_VERSION))
            {
                throw new Exception($"Mapper version is not defined in application settings. Please upgrade to the latest version of GameHook.");
            }

            var localMapperVersion = string.Empty;
            if (File.Exists(MapperEnvironment.MapperLocalCommitHashFilePath))
            {
                localMapperVersion = await File.ReadAllTextAsync(MapperEnvironment.MapperLocalCommitHashFilePath);
            }

            if (_appSettings.MAPPER_VERSION != localMapperVersion)
            {
                _logger.LogInformation($"Downloading new mappers from server.");

                var httpClient = _httpClientFactory.CreateClient();

                await DownloadMappers(httpClient, $"https://github.com/gamehook-io/mappers/archive/{_appSettings.MAPPER_VERSION}.zip");
                await File.WriteAllTextAsync(MapperEnvironment.MapperLocalCommitHashFilePath, _appSettings.MAPPER_VERSION);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not perform update check for mappers.");

            return false;
        }
    }
    private void WriteTextToFile(string filepath, string text, DateTime? created = null, DateTime? updated = null)
    {
        if (!string.IsNullOrWhiteSpace(text))  
        {
            try
            {
                var file = new FileInfo(filepath);
                file.Directory?.Create();
                File.WriteAllText(file.FullName, text);
                File.SetCreationTimeUtc(file.FullName, created ?? DateTime.Now);
                File.SetLastAccessTimeUtc(file.FullName, updated ?? DateTime.Now);
                File.SetLastWriteTimeUtc(file.FullName, updated ?? DateTime.Now);
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to write {filepath} because of an exception.");
                return;
            }
        }
        _logger.LogWarning($"Failed to write {filepath} because the input data was blank.");
    }
    public async Task SaveUpdatedMappersAsync(List<UpdateMapperDto> updatedMappers)
    {
        foreach (var mapper in updatedMappers)
        {
            var mapperPath = $"{MapperEnvironment.MapperLocalDirectory.Replace("\\", "/")}/{mapper.RelativeXmlPath}";
            var jsPath = $"{MapperEnvironment.MapperLocalDirectory.Replace("\\", "/")}/{mapper.RelativeJsPath}";
            _mapperArchiveManager.ArchiveFile(mapper.RelativeXmlPath, 
                mapperPath);
            _mapperArchiveManager.ArchiveFile(mapper.RelativeJsPath, 
                jsPath);
            WriteTextToFile(mapperPath, mapper.XmlData, mapper.Created, mapper.Updated);
            WriteTextToFile(jsPath, mapper.JsData, mapper.Created, mapper.Updated);
        }
        var archiveFolder = MapperEnvironment.MapperArchiveDirectory;
        _mapperArchiveManager.ArchiveDirectory(archiveFolder);
        //Update the mapper list
        var mapperTree = MapperTreeUtility.GenerateMapperDtoTree(MapperEnvironment.MapperLocalDirectory);
        MapperTreeUtility.SaveChanges(MapperEnvironment.MapperLocalDirectory, mapperTree);
        //Finish off by checking for any changes
        await CheckForUpdates();
    }
}
