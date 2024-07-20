using System.Text.Json;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Domain.Models;
using GameHook.Domain.Models.Mappers;
using GameHook.Mappers;
using PokeAByte.Web.Models;

namespace PokeAByte.Web.Services;

public class MapperManagerService(
    IMapperUpdateManager mapperUpdateManager,
    IMapperArchiveManager mapperArchiveManager,
    ILogger<MapperManagerService> logger,
    IGithubRestApi githubRestApi,
    MapperClientService mapperClientService)
{
    private MapperClientService _mapperClientService = mapperClientService;
    private static string MapperLocalDirectory => 
        Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers");

    public async Task<bool> CheckForUpdatesAsync()
    {
        var mapperTree = MapperTreeUtility.GenerateMapperDtoTree(MapperLocalDirectory);
        MapperTreeUtility.SaveChanges(MapperLocalDirectory, mapperTree);
        var updatesFound = await mapperUpdateManager.CheckForUpdates();
        return updatesFound;
    }

    public IEnumerable<MapperComparisonDto> GetMapperUpdates()
    {
        if (!File.Exists(MapperEnvironment.OutdatedMapperTreeJson))
        {
            logger.LogError($"{MapperEnvironment.OutdatedMapperTreeJson} does not exist locally.");
            return [];
        }
        //load the mapper list
        var jsonStr = File.ReadAllText(MapperEnvironment.OutdatedMapperTreeJson);
        if (string.IsNullOrWhiteSpace(jsonStr))
            logger.LogError($"{MapperEnvironment.OutdatedMapperTreeJson} was empty.");
        try
        {
            return JsonSerializer.Deserialize<IEnumerable<MapperComparisonDto>>(jsonStr) ?? [];
        }
        catch (Exception e)
        {
            logger.LogError(e,"Exception!");
            return [];
        }
    }

    private Action<int>? _updateProcessedCountAction;
    public async Task DownloadMapperUpdatesAsync(IEnumerable<MapperComparisonDto> mappers,
        Action<int>? updateProcessedCountAction = null)
    {
        try
        {
            if (updateProcessedCountAction is not null)
                _updateProcessedCountAction = updateProcessedCountAction;
            var mapperDownloads = mappers
                .Select(m => 
                    m.LatestVersion ?? 
                    (m.CurrentVersion ?? 
                     throw new InvalidOperationException("Current Version and Latest version are both null.")))
                .ToList();
            await githubRestApi.DownloadMapperFiles(mapperDownloads, 
                mapperUpdateManager.SaveUpdatedMappersAsync,
                _updateProcessedCountAction);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to download mappers.");
        }
    }

    public List<MapperDto> GetMapperList()
    {
        var mapperTree = MapperTreeUtility.GenerateMapperDtoTree(MapperLocalDirectory);
        MapperTreeUtility.SaveChanges(MapperLocalDirectory, mapperTree);
        return mapperTree;
    }

    public IReadOnlyDictionary<string, IReadOnlyList<ArchivedMapperDto>>? GetArchivedMappers()
    {
        try
        {
            return mapperArchiveManager.GetArchivedMappers();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get archived mappers list!");
            return null;
        }
    }

    public void RestoreArchivedMappers(IEnumerable<ArchivedMapperDto> archivedMappers)
    {
        try
        {
            mapperArchiveManager.RestoreMappersFromArchive(archivedMappers.ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to restore archived mappers!");
        }
    }

    public void DeleteArchivedMappers(IEnumerable<ArchivedMapperDto> archivedMappers)
    {
        try
        {
            mapperArchiveManager.DeleteMappersFromArchive(archivedMappers.ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete archived mappers!");
        }
    }

    public List<MapperArchiveModel> CreateArchiveList()
    {
        mapperArchiveManager.GenerateArchivedList();
        var mappers = GetArchivedMappers();
        var mapperModels = mappers?
            .GroupBy(g => GetBasePathFromArchive(g.Key))
            .Select(m => new MapperArchiveModel
            {
                BasePath = m.Key,
                MapperModels = m.SelectMany(c => c.Value.ToList()).ToList()
            })
            .ToList();
        return mapperModels ?? [];   
    }

    private static string GetBasePathFromArchive(string fullPath)
    {
        var barePath = fullPath[(MapperEnvironment.MapperLocalArchiveDirectory.Length+1)..];
        return "/" + barePath[..barePath.IndexOf('/')];
    }
    
    public void ArchiveMappers(List<MapperDto> mappers)
    {
        foreach (var mapper in mappers)
        {
            var relativeJsPath = mapper.Path
                [..mapper.Path.IndexOf(".xml", StringComparison.Ordinal)] + ".js";
            var mapperPath = $"{MapperEnvironment.MapperLocalDirectory
                .Replace("\\", "/")}/{mapper.Path}";
            var jsPath = $"{MapperEnvironment.MapperLocalDirectory
                .Replace("\\", "/")}/{relativeJsPath}";
            mapperArchiveManager.ArchiveFile(mapper.Path, mapperPath);
            mapperArchiveManager.ArchiveFile(relativeJsPath, jsPath);
        }
        var archiveFolder = MapperEnvironment.MapperArchiveDirectory;
        mapperArchiveManager.ArchiveDirectory(archiveFolder);
        //Update the mapper list
        var mapperTree = MapperTreeUtility.GenerateMapperDtoTree(MapperEnvironment.MapperLocalDirectory);
        MapperTreeUtility.SaveChanges(MapperEnvironment.MapperLocalDirectory, mapperTree);
    }
}