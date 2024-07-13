﻿using System.Text.Json;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Domain.Models;
using GameHook.Domain.Models.Mappers;
using GameHook.Mappers;

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
        var jsonStr = System.IO.File.ReadAllText(MapperEnvironment.OutdatedMapperTreeJson);
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

    public async Task DownloadMapperUpdatesAsync(IEnumerable<MapperComparisonDto> mappers)
    {
        try
        {
            var mapperDownloads = mappers
                .Select(m => 
                    m.LatestVersion ?? 
                    (m.CurrentVersion ?? 
                     throw new InvalidOperationException("Current Version and Latest version are both null.")))
                .ToList();
            await githubRestApi.DownloadMapperFiles(mapperDownloads, mapperUpdateManager.SaveUpdatedMappersAsync);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to download mappers.");
        }
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

    public IReadOnlyDictionary<string, IReadOnlyList<ArchivedMapperDto>>? RefreshArchivedMappersList()
    {
        mapperArchiveManager.GenerateArchivedList();
        return GetArchivedMappers();
    }
    
}