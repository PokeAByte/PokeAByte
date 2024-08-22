using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PokeAByte.Application.Mappers;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Web.Models;

namespace PokeAByte.Web.Services.Mapper;

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

    public void SaveArchivedItem(MapperArchiveModel item)
    {
        var mappers = GenerateArchiveHelper();
        if (mappers is null)
            return;
        //Find the item
        var found = mappers.FirstOrDefault(x => x.Hash == item.Hash);
        //Update the found mapper
        if (found is null) return;
        found.DisplayName = item.DisplayName;
        found.Type = item.Type == ArchiveType.None ? FindTyping(item) : item.Type;
        SaveArchivedData(mappers);
    }
    public List<MapperArchiveModel> CreateArchiveList()
    {
        var mapperModels = GenerateArchiveHelper();
        if(mapperModels is not null)
            SaveArchivedData(mapperModels);
        return mapperModels ?? [];   
    }

    private List<MapperArchiveModel>? GenerateArchiveHelper()
    {
        mapperArchiveManager.GenerateArchivedList();
        var mappers = GetArchivedMappers();
        return mappers?
            .GroupBy(g => GetBasePathFromArchive(g.Key))
            .Select(m =>
            {
                var data = m
                    .SelectMany(c => c.Value
                        .Select(v => v.FullPath))
                    .ToList();
                var hash = CreateMd5Hash(string.Join("", data));
                var name = GetDisplayName(hash, m.Key);
                var type = GetTyping(hash, m.SelectMany(c => c.Value).ToList());
                
                return new MapperArchiveModel
                {
                    Hash = hash,
                    DisplayName = name,
                    BasePath = m.Key,
                    Type = type,
                    MapperModels = m.SelectMany(c => c.Value.ToList()).ToList()
                };
            })
            .ToList();
    }

    private static string CreateMd5Hash(string input)
    {
        // Convert the input string to a byte array and compute the hash.
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }
    private string GetDisplayName(string hash, string key)
    {
        if (!File.Exists(MapperEnvironment.MapperArchiveSavedDataJson))
            return "";
        try
        {
            var jsonStr = File.ReadAllText(MapperEnvironment.MapperArchiveSavedDataJson);
            var data = JsonSerializer.Deserialize<IDictionary<string, MapperArchiveDto>>(jsonStr);
            if (data is null)
                return "";
            var foundHash = data
                .FirstOrDefault(h => h.Key == hash);
            if (foundHash.Key is null || foundHash.Key != hash)
                return "";
            return foundHash.Value.DisplayName;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to read mapper archive saved data!");
            return "";
        }
    }
    private ArchiveType GetTyping(string hash, List<ArchivedMapperDto> list)
    {
        if (!File.Exists(MapperEnvironment.MapperArchiveSavedDataJson))
            return ArchiveType.None;
        try
        {
            var jsonStr = File.ReadAllText(MapperEnvironment.MapperArchiveSavedDataJson);
            var data = JsonSerializer.Deserialize<IDictionary<string, MapperArchiveDto>>(jsonStr);
            if (data is null)
                return ArchiveType.None;
            var foundHash = data
                .FirstOrDefault(h => h.Key == hash);
            if (foundHash.Key is null || foundHash.Key != hash)
                return ArchiveType.None;
            return foundHash.Value.Type;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to read mapper archive saved data!");
            return ArchiveType.None;;
        }  
    }
    private void SaveArchivedData(List<MapperArchiveModel> data)
    {
        var mapperDict = data
            .Select(CreateSavedData)
            .ToDictionary();
        try
        {
            var json = JsonSerializer.Serialize(mapperDict);
            File.WriteAllText(MapperEnvironment.MapperArchiveSavedDataJson, json);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save archived data.");
        }
    }
    
    private KeyValuePair<string, MapperArchiveDto> CreateSavedData(MapperArchiveModel data)
    {
        var paths = data.MapperModels
            .Select(d => d.FullPath)
            .ToList();
        var hash = CreateMd5Hash(string.Join("", paths));
        var type = data.Type;
        if(type == ArchiveType.None)
            type = FindTyping(data);
        return new KeyValuePair<string, MapperArchiveDto>(hash, new MapperArchiveDto
        {
            BasePath = data.BasePath,
            DisplayName = data.DisplayName,
            Hash = hash,
            Type = type
        });
    }

    private ArchiveType FindTyping(MapperArchiveModel data)
    {
        //See if the mapper exists currently
        var failedToFind = data
            .MapperModels
            .Select(mapper => $"{MapperEnvironment.MapperLocalDirectory}{mapper.PathDisplayName}/{mapper.Mapper.DisplayName}")
            .Any(localCopy => !File.Exists(localCopy));
        return failedToFind ? //cheat and say it we archived them 
            ArchiveType.Archived :
            //cheat and say we backed it up
            ArchiveType.BackUp;
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

    public void BackupMappers(List<MapperDto> mappers)
    {
        foreach (var mapper in mappers)
        {
            var relativeJsPath = mapper.Path
                [..mapper.Path.IndexOf(".xml", StringComparison.Ordinal)] + ".js";
            var mapperPath = $"{MapperEnvironment.MapperLocalDirectory
                .Replace("\\", "/")}/{mapper.Path}";
            var jsPath = $"{MapperEnvironment.MapperLocalDirectory
                .Replace("\\", "/")}/{relativeJsPath}";
            mapperArchiveManager.BackupFile(mapper.Path, mapperPath);
            mapperArchiveManager.BackupFile(relativeJsPath, jsPath);
        }
        var archiveFolder = MapperEnvironment.MapperArchiveDirectory;
        mapperArchiveManager.ArchiveDirectory(archiveFolder);
        //Update the mapper list
        var mapperTree = MapperTreeUtility.GenerateMapperDtoTree(MapperEnvironment.MapperLocalDirectory);
        MapperTreeUtility.SaveChanges(MapperEnvironment.MapperLocalDirectory, mapperTree);
    }
}