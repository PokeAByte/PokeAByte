using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Services.Mapper;
using PokeAByte.Domain.Services.MapperFile;
using PokeAByte.Web.Helper;

namespace PokeAByte.Web;

public class RestAPI { }

public static class FilesEndpoints
{
    public static void MapFilesEndpoints(this WebApplication app)
    {
        app.MapGet("/files/mappers", GetMapperFiles);
        app.MapGet("/files/mapper/check_for_updates", CheckForUpdatesAsync);
        app.MapGet("/files/mapper/get_updates", GetUpdatesAsync);
        app.MapPost("/files/mapper/download_updates", DownloadMapperUpdatesAsync);
        app.MapGet("/files/mapper/get_archived", GetArchivedMappersAsync);
        app.MapPost("/files/mapper/archive_mappers", ArchiveMappers);
        app.MapGet("/files/open_mapper_archive_folder", () => XPlatHelper.OpenFileManager(MapperPaths.MapperLocalArchiveDirectory));
        app.MapGet("/files/open_mapper_folder", () => XPlatHelper.OpenFileManager(MapperPaths.MapperDirectory));
        app.MapPost("/files/mapper/delete_mappers", DeleteMappers);
        app.MapPost("/files/mapper/restore_mappers", RestoreMapper);
    }

    public static void DeleteMappers(
        MapperFileService mapperFileService,
        [FromBody] IEnumerable<ArchivedMapperDto> archivedMappers)
    {
        mapperFileService.DeleteMappersFromArchive(archivedMappers);
    }

    public static void RestoreMapper(IMapperArchiveManager archive, IEnumerable<ArchivedMapperDto> archivedMappers)
    {
        archive.RestoreMappersFromArchive(archivedMappers.ToList());
    }

    public static IResult ArchiveMappers(
        IMapperArchiveManager archiveManager,
        MapperFileService mapperFileService,
        List<MapperDto> mappers)
    {
        foreach (var mapper in mappers)
        {
            var relativeJsPath = mapper.Path
                [..mapper.Path.IndexOf(".xml", StringComparison.Ordinal)] + ".js";
            var mapperPath = $"{MapperPaths.MapperDirectory.Replace("\\", "/")}/{mapper.Path}";
            var jsPath = $"{MapperPaths.MapperDirectory.Replace("\\", "/")}/{relativeJsPath}";
            archiveManager.ArchiveFile(mapper.Path, mapperPath);
            archiveManager.ArchiveFile(relativeJsPath, jsPath);
        }
        var archiveFolder = MapperPaths.MapperArchiveDirectory;
        archiveManager.ArchiveDirectory(archiveFolder);
        //Update the mapper list
        var mapperTree = MapperTreeUtility.GenerateMapperDtoTree(MapperPaths.MapperDirectory);
        MapperTreeUtility.SaveChanges(MapperPaths.MapperDirectory, mapperTree);
        mapperFileService.Refresh();
        return TypedResults.Ok();
    }

    public static Dictionary<string, IEnumerable<ArchivedMapperDto>> GetArchivedMappersAsync(
        MapperFileService mapperFileService)
    {
        return mapperFileService.ListArchived()
            .GroupBy(x => x.FullPath)
            .Select(
                x => new KeyValuePair<string, IEnumerable<ArchivedMapperDto>>(x.Key, x)
            )
            .ToDictionary();
    }


    public static async Task<IResult> DownloadMapperUpdatesAsync(
        IMapperUpdateManager updateManager,
        IGithubRestApi githubRest,
        MapperFileService mapperFileService,
        [FromBody] IEnumerable<MapperComparisonDto> mappers)
    {
        var mapperDownloads = mappers
            .Select(m => m.LatestVersion ??
                (m.CurrentVersion ?? throw new InvalidOperationException("Current Version and Latest version are both null."))
            )
            .ToList();
        await githubRest.DownloadMapperFiles(mapperDownloads, updateManager.SaveUpdatedMappersAsync);
        mapperFileService.Refresh();
        return TypedResults.Ok();
    }

    public static IEnumerable<MapperFileModel> GetMapperFiles(MapperFileService mapperFileService)
    {
        return mapperFileService.ListInstalled().Select(x => new MapperFileModel(x.Id, x.DisplayName));
    }

    public static async Task<bool> CheckForUpdatesAsync(IMapperUpdateManager updateManager)
    {
        var mapperTree = MapperTreeUtility.GenerateMapperDtoTree(MapperPaths.MapperDirectory);
        MapperTreeUtility.SaveChanges(MapperPaths.MapperDirectory, mapperTree);
        //check for updates
        var updatesFound = await updateManager.CheckForUpdates();
        return updatesFound;
    }

    public static async Task<IResult> GetUpdatesAsync(IMapperUpdateManager updateManager)
    {
        await updateManager.CheckForUpdates();
        if (!File.Exists(MapperPaths.OutdatedMapperTreeJson))
        {
            return TypedResults.BadRequest($"{MapperPaths.OutdatedMapperTreeJson} does not exist locally.");
        }
        //load the mapper list
        var jsonStr = File.ReadAllText(MapperPaths.OutdatedMapperTreeJson);
        if (string.IsNullOrWhiteSpace(jsonStr))
            return TypedResults.BadRequest($"{MapperPaths.OutdatedMapperTreeJson} was empty.");

        return TypedResults.Ok(
            JsonSerializer.Deserialize(jsonStr, ApiJsonContext.Default.IEnumerableMapperComparisonDto)
        );
    }
}
