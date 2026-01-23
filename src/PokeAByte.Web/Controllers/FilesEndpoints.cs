using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Web.Helper;

namespace PokeAByte.Web;

public class RestAPI { }

public static class FilesEndpoints
{
    public static void MapFilesEndpoints(this WebApplication app)
    {
        app.MapGet("/files/mappers", GetMappers);
        app.MapGet("/files/mapper/check_for_updates", CheckForUpdatesAsync);
        app.MapGet("/files/mapper/get_updates", GetUpdatesAsync);
        app.MapPost("/files/mapper/download_updates", DownloadMapperUpdatesAsync);
        app.MapGet("/files/mapper/get_archived", GetArchivedMappersAsync);
        app.MapPost("/files/mapper/archive_mappers", ArchiveMappers);
        app.MapPost("/files/mapper/backup_mappers", BackupMappers);
        app.MapGet("/files/open_mapper_archive_folder", () => XPlatHelper.OpenFileManager(MapperService.MapperArchivePath));
        app.MapGet("/files/open_mapper_folder", () => XPlatHelper.OpenFileManager(MapperService.MapperDirectory));
        app.MapPost("/files/mapper/delete_mappers", DeleteMappers);
        app.MapPost("/files/mapper/restore_mappers", RestoreMapper);
    }

    public static void DeleteMappers(IMapperService mapperService, [FromBody] string archivePath)
    {
        mapperService.DeleteArchive(archivePath);
    }

    public static void RestoreMapper(IMapperService mapperService, [FromBody] string archivePath)
    {
        mapperService.Restore(archivePath);
    }

    public static IResult ArchiveMappers(IMapperService mapperService, List<string> mappers)
    {
        return mapperService.Archive(mappers)
            ? TypedResults.Ok()
            : TypedResults.InternalServerError();
    }

    public static async Task<IResult> BackupMappers(IMapperService mapperService, List<string> mappers)
    {
        return await mapperService.Backup(mappers)
            ? TypedResults.Ok()
            : TypedResults.InternalServerError();
    }

    public static Dictionary<string, IEnumerable<ArchivedMapperFile>> GetArchivedMappersAsync(
        IMapperService mapperService)
    {
        return mapperService.ListArchived()
            .GroupBy(x => x.Path)
            .Select(
                x => new KeyValuePair<string, IEnumerable<ArchivedMapperFile>>(x.Key, x)
            )
            .ToDictionary();
    }


    public static async Task<IResult> DownloadMapperUpdatesAsync(
        IMapperService mapperService, 
        [FromBody] List<string> mapperPaths)
    {
        if (await mapperService.DownloadAsync(mapperPaths))
        {
            return TypedResults.Ok();
        }
        return TypedResults.InternalServerError();
    }

    public static IEnumerable<InstalledMapper> GetMappers(IMapperService mapperService)
    {
        return mapperService.ListInstalled();
    }

    public static bool CheckForUpdatesAsync(IMapperService mapperService)
        => mapperService.UpdateRemoteMappers();

    public static async Task<IEnumerable<RemoteMapperFile>> GetUpdatesAsync(IMapperService mapperService)
        => mapperService.ListRemote();
}
