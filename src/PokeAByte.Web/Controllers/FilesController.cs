using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Services.Mapper;
using PokeAByte.Domain.Services.MapperFile;
using PokeAByte.Infrastructure.Github;
using PokeAByte.Web.Helper;

namespace PokeAByte.Web.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("files")]
    public class FilesController(
        ILogger<FilesController> logger,
        IMapperUpdateManager updateManager,
        IGithubRestApi githubRest,
        IMapperArchiveManager archiveManager,
        MapperFileService mapperFileService,
        IGithubApiSettings githubApiSettings) : ControllerBase
    {
        private static string MapperLocalDirectory => Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers");

        [HttpGet("mappers")]
        public ActionResult<IEnumerable<MapperFileModel>> GetMapperFiles()
        {
            return Ok(mapperFileService.ListInstalled().Select(x => new MapperFileModel()
            {
                Id = x.Id,
                DisplayName = x.DisplayName
            }));
        }

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

        [HttpGet("mapper/get_updates")]
        public ActionResult<IEnumerable<MapperDto>> GetMapperUpdatesAsync()
        {
            updateManager.CheckForUpdates();
            if (!System.IO.File.Exists(MapperPaths.OutdatedMapperTreeJson))
            {
                return ApiHelper.BadRequestResult($"{MapperPaths.OutdatedMapperTreeJson} does not exist locally.");
            }
            //load the mapper list
            var jsonStr = System.IO.File.ReadAllText(MapperPaths.OutdatedMapperTreeJson);
            if (string.IsNullOrWhiteSpace(jsonStr))
                return ApiHelper.BadRequestResult($"{MapperPaths.OutdatedMapperTreeJson} was empty.");
            try
            {
                return Ok(JsonSerializer.Deserialize<IEnumerable<MapperComparisonDto>>(jsonStr));
            }
            catch (Exception e)
            {
                return ApiHelper.BadRequestResult(e.ToString());
            }
        }

        [HttpPost("mapper/download_updates")]
        public async Task<ActionResult> DownloadMapperUpdatesAsync(IEnumerable<MapperComparisonDto> mappers)
        {
            try
            {
                var mapperDownloads = mappers
                    .Select(m => m.LatestVersion ??
                        (m.CurrentVersion ?? throw new InvalidOperationException("Current Version and Latest version are both null."))
                    )
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

        [HttpGet("mapper/get_archived")]
        public ActionResult GetArchivedMappersAsync()
        {
            return Ok(
                mapperFileService.ListArchived()
                    .GroupBy(x => x.FullPath)
                    .Select(
                        x => new KeyValuePair<string, IEnumerable<ArchivedMapperDto>>(x.Key, x)
                    )
                    .ToDictionary()
            );
        }

        [HttpPost("mapper/archive_mappers")]
        public ActionResult ArchiveMappers(List<MapperDto> mappers)
        {
            try
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
                return Ok();
            }
            catch (Exception e)
            {
                return ApiHelper.BadRequestResult($"Exception: {e}");
            }
        }

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

        [HttpPost("mapper/delete_mappers")]
        public ActionResult DeleteMappers(IEnumerable<ArchivedMapperDto> archivedMappers)
        {
            mapperFileService.DeleteMappersFromArchive(archivedMappers);
            return Ok();
        }

        [HttpGet("get_github_settings")]
        public ActionResult GetGithubSettings()
        {
            return Ok(JsonSerializer.Serialize((GithubApiSettings)githubApiSettings));
        }

        [HttpPost("save_github_settings")]
        public async Task<ActionResult> SaveGithubSettingsAsync(GithubApiSettings settings)
        {
            try
            {
                await CheckForUpdates();
                githubApiSettings.CopySettings(settings);
                githubApiSettings.SaveChanges();

                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to save github settings because of an exception.");
                return ApiHelper.BadRequestResult("Failed to save github settings changes because of an exception. " +
                                                  $"{e}");
            }
        }

        [HttpGet("test_github_settings")]
        public async Task<ActionResult> TestGithubSettingsAsync()
        {
            try
            {
                var result = await githubRest.TestSettings();
                return string.IsNullOrWhiteSpace(result)
                    ? Ok("Successfully connected to Github Api!")
                    : Ok($"Failed to connect to Github Api - Reason: {result}");
            }
            catch (Exception e)
            {
                return Ok($"Failed to connect to Github Api - Reason: {e}");
            }
        }

        [HttpGet("open_mapper_folder")]
        public ActionResult OpenMapperFolder()
        {
            try
            {
                XPlatHelper.OpenFileManager(MapperPaths.MapperDirectory);
                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to open directory.");
                return ApiHelper.BadRequestResult(e.ToString());
            }
        }

        [HttpGet("open_mapper_archive_folder")]
        public ActionResult OpenMapperArchiveFolder()
        {
            XPlatHelper.OpenFileManager(MapperPaths.MapperLocalArchiveDirectory);
            return Ok();
        }

        [HttpGet("get_github_link")]
        public ActionResult GetGithubLink()
        {
            return Ok(githubApiSettings.GetGithubUrl());
        }
    }
}