using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Services.MapperFile;

namespace PokeAByte.Domain.Services.Mapper;

[JsonSerializable(typeof(List<MapperDto>))]
[JsonSerializable(typeof(List<MapperComparisonDto>))]
public partial class ManagerJsonContext : JsonSerializerContext
{
}

public class MapperUpdateManager : IMapperUpdateManager
{
    private readonly ILogger<MapperUpdateManager> _logger;
    private readonly AppSettings _appSettings;
    private readonly IMapperFileService _mapperFileService;
    private readonly MapperUpdaterSettings _mapperUpdaterSettings;
    private readonly IGithubRestApi _githubRestApi;

    public MapperUpdateManager(ILogger<MapperUpdateManager> logger,
        AppSettings appSettings,
        MapperUpdaterSettings mapperUpdaterSettingsManager,
        IMapperFileService mapperFileService,
        IGithubRestApi githubRestApi)
    {
        _logger = logger;
        _appSettings = appSettings;
        _mapperFileService = mapperFileService;
        _mapperUpdaterSettings = mapperUpdaterSettingsManager;
        _githubRestApi = githubRestApi;

        if (Directory.Exists(BuildEnvironment.ConfigurationDirectory) == false)
        {
            _logger.LogInformation("Creating configuration directory for Poke-A-Byte.");

            Directory.CreateDirectory(BuildEnvironment.ConfigurationDirectory);
        }
    }

    //Returns a list of outdated mappers 
    private async Task<List<MapperComparisonDto>> GetOutdatedMapperList()
    {
        //Get the latest version of the mapper tree from github
        var mapperListResponse = await _githubRestApi.GetMapperTreeFile();
        if (mapperListResponse is null)
        {
            _logger.LogError("Failed to download the latest version of the mapper tree json from Github.");
            return [];
        }
        //Convert the remote data to a MapperDto
        var remoteDeserialized = await mapperListResponse.ReadFromJsonAsync(ManagerJsonContext.Default.ListMapperDto);
        if (remoteDeserialized is null || remoteDeserialized.Count == 0)
        {
            _logger.LogError("Could not read the remote json file.");
            return [];
        }
        var remote = remoteDeserialized
            .Select(m => m with { Path = m.Path.Trim('/').Trim('\\') })
            .ToList();
        //Get the current version the user has on their filesystem
        var mapperTree = MapperTreeUtility
            .Load(MapperPaths.MapperDirectory)
            .Select(m => m with { Path = m.Path.Trim('/').Trim('\\') })
            .ToList();
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
            var jsonData = JsonSerializer.Serialize(outdatedMappers, ManagerJsonContext.Default.ListMapperComparisonDto);
            await File.WriteAllTextAsync(MapperPaths.OutdatedMapperTreeJson, jsonData);
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
            var mapperPath = $"{MapperPaths.MapperDirectory.Replace("\\", "/")}/{mapper.RelativeXmlPath}";
            var jsPath = $"{MapperPaths.MapperDirectory.Replace("\\", "/")}/{mapper.RelativeJsPath}";
            _mapperFileService.ArchiveFile(mapper.RelativeXmlPath, mapperPath);
            _mapperFileService.ArchiveFile(mapper.RelativeJsPath, jsPath);
            WriteTextToFile(mapperPath, mapper.XmlData, mapper.Created, mapper.Updated);
            WriteTextToFile(jsPath, mapper.JsData, mapper.Created, mapper.Updated);
        }
        var archiveFolder = MapperPaths.MapperArchiveDirectory;
        _mapperFileService.ArchiveDirectory(archiveFolder);
        _mapperFileService.Refresh();
        // Finish off by checking for any changes
        await CheckForUpdates();
    }
}
