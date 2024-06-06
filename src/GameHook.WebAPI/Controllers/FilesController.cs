using System.Text.Json;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Infrastructure.Github;
using GameHook.Mappers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GameHook.WebAPI.Controllers
{
    public class MapperFileModel
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("files")]
    public class FilesController(IMapperFilesystemProvider mapperFilesystemProvider,
        ILogger<FilesController> logger, IMapperUpdateManager updateManager, 
        IGithubRestApi githubRest, IMapperArchiveManager archiveManager) : ControllerBase
    {        
        private static string MapperLocalDirectory => 
            Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers");
        public IMapperFilesystemProvider MapperFilesystemProvider { get; } = mapperFilesystemProvider;

        [SwaggerOperation("Returns a list of all mapper files available inside of the /mappers folder.")]
        [HttpGet("mappers")]
        public ActionResult<IEnumerable<MapperFileModel>> GetMapperFiles()
        {
            MapperFilesystemProvider.CacheMapperFiles();

            return Ok(MapperFilesystemProvider.MapperFiles.Select(x => new MapperFileModel()
            {
                Id = x.Id,
                DisplayName = x.DisplayName
            }));
        }

        [SwaggerOperation("Forces an update check.")]
        [HttpGet("mapper/check_for_updates")]
        public async Task<ActionResult<bool>> CheckForUpdates()
        {
            //rebuild the mapper tree
            //var mapperTreeUtil = new MapperTreeUtility(MapperLocalDirectory);
            var mapperTree = MapperTreeUtility.GenerateMapperDtoTree(MapperLocalDirectory);
            MapperTreeUtility.SaveChanges(MapperLocalDirectory, mapperTree);
            //check for updates
            var updatesFound = await updateManager.CheckForUpdates();
            return updatesFound ? Ok(true) : Ok(false);
        }
        
        [SwaggerOperation("Returns a list of available mapper updates.")]
        [HttpGet("mapper/get_updates")]
        public ActionResult<IEnumerable<MapperDto>> GetMapperUpdatesAsync()
        {
            if (!System.IO.File.Exists(MapperEnvironment.OutdatedMapperTreeJson))
            {
                return ApiHelper.BadRequestResult($"{MapperEnvironment.OutdatedMapperTreeJson} does not exist locally.");
            }
            //load the mapper list
            var jsonStr = System.IO.File.ReadAllText(MapperEnvironment.OutdatedMapperTreeJson);
            if (string.IsNullOrWhiteSpace(jsonStr))
                return ApiHelper.BadRequestResult($"{MapperEnvironment.OutdatedMapperTreeJson} was empty.");
            try
            {
                return Ok(JsonSerializer.Deserialize<IEnumerable<MapperComparisonDto>>(jsonStr));
            }
            catch (Exception e)
            {
                return ApiHelper.BadRequestResult(e.ToString());
            }
        }

        [SwaggerOperation("Returns a list of available mapper updates.")]
        [HttpPost("mapper/download_updates")]
        public async Task<ActionResult> DownloadMapperUpdatesAsync(IEnumerable<MapperComparisonDto> mappers)
        {
            try
            {
                var mapperDownloads = mappers
                    .Select(m => 
                        m.LatestVersion ?? 
                        (m.CurrentVersion ?? 
                         throw new InvalidOperationException("Current Version and Latest version are both null.")))
                    .ToList();
                await githubRest.DownloadMapperFiles(mapperDownloads, updateManager.SaveUpdatedMappersAsync);
                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to download mappers.");
                return ApiHelper.BadRequestResult("Failed to download mappers.");
            }
        }
        [SwaggerOperation("Returns a list of available mapper updates.")]
        [HttpGet("mapper/get_archived")]
        public ActionResult GetArchivedMappersAsync()
        {
            try
            {
                return Ok(archiveManager.GetArchivedMappers());
            }
            catch (Exception e)
            {
                return ApiHelper.BadRequestResult(e.ToString());
            }
        }

        [SwaggerOperation("Restores a list of mappers.")]
        [HttpPost("mapper/restore_mappers")]
        public ActionResult RestoreMapper(IEnumerable<ArchivedMapperDto> archivedMappers)
        {
            try
            {
                archiveManager.RestoreMappersFromArchive(archivedMappers.ToList());
                return Ok();
            }
            catch (Exception e)
            {
                return ApiHelper.BadRequestResult($"Exception: {e}");
            }
        }
        [SwaggerOperation("Restores a list of mappers.")]
        [HttpPost("mapper/delete_mappers")]
        public ActionResult DeleteMappers(IEnumerable<ArchivedMapperDto> archivedMappers)
        {
            try
            {
                archiveManager.DeleteMappersFromArchive(archivedMappers.ToList());
                return Ok();
            }
            catch (Exception e)
            {
                return ApiHelper.BadRequestResult($"Exception: {e}");
            }
        }
        [SwaggerOperation("Refreshes the list of archived files.")]
        [HttpGet("mapper/refresh_archived_list")]
        public ActionResult RefreshArchivedList()
        {
            archiveManager.GenerateArchivedList();
            return Ok(archiveManager.GetArchivedMappers());
        }
    }
}