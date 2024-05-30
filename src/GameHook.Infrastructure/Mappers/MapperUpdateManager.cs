using System.IO.Compression;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameHook.Infrastructure.Mappers
{
    public class MapperUpdateManager : IMapperUpdateManager
    {
        private readonly ILogger<MapperUpdateManager> _logger;
        private readonly AppSettings _appSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MapperSettingsManager _mapperSettingsManager;

        public MapperUpdateManager(ILogger<MapperUpdateManager> logger, 
            AppSettings appSettings, 
            IHttpClientFactory httpClientFactory,
            MapperSettingsManager mapperSettingsManager)
        {
            _logger = logger;
            _appSettings = appSettings;
            _httpClientFactory = httpClientFactory;
            _mapperSettingsManager = mapperSettingsManager;

            if (Directory.Exists(BuildEnvironment.ConfigurationDirectory) == false)
            {
                _logger.LogInformation("Creating configuration directory for GameHook.");

                Directory.CreateDirectory(BuildEnvironment.ConfigurationDirectory);
            }
        }
        
        private static string MapperLocalDirectory => 
            Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers");
        private static string MapperLocalArchiveDirectory => 
            Path.Combine(BuildEnvironment.ConfigurationDirectory, "MapperArchives");
        private static string MapperLocalCommitHashFilePath => 
            Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers", "COMMIT_HASH.txt");
        private static string MapperTemporaryZipFilepath => 
            Path.Combine(BuildEnvironment.ConfigurationDirectory, $"mappers_tmp.zip");
        private static string MapperTemporaryExtractionDirectory => 
            Path.Combine(BuildEnvironment.ConfigurationDirectory, $"mappers_tmp\\");
        private static void CleanupTemporaryFiles()
        {
            // TODO: 2/8/2024 - Remove this at a future date.
            var oldMapperJsonFile = Path.Combine(BuildEnvironment.ConfigurationDirectory, "mappers.json");
            if (File.Exists(oldMapperJsonFile))
            {
                File.Delete(oldMapperJsonFile);
            }

            if (File.Exists(MapperTemporaryZipFilepath))
            {
                File.Delete(MapperTemporaryZipFilepath);
            }

            if (Directory.Exists(MapperTemporaryExtractionDirectory))
            {
                Directory.Delete(MapperTemporaryExtractionDirectory, true);
            }
        }

        private static async Task DownloadMappers(HttpClient httpClient, string distUrl)
        {
            try
            {
                CleanupTemporaryFiles();

                // Download the ZIP from Github.
                var bytes = await httpClient.GetByteArrayAsync(distUrl);
                await File.WriteAllBytesAsync(MapperTemporaryZipFilepath, bytes);

                // Extract to the temporary directory.
                using var zout = ZipFile.OpenRead(MapperTemporaryZipFilepath);
                zout.ExtractToDirectory(MapperTemporaryExtractionDirectory);

                var mapperTemporaryExtractionSubfolderDirectory = Directory.GetDirectories(MapperTemporaryExtractionDirectory).FirstOrDefault() ??
                    throw new Exception("Mappers were downloaded from the server, but did not contain a subfolder.");

                if (Directory.Exists(MapperLocalDirectory))
                {
                    //make a zipped archived of the old mappers
                    ZipFile.CreateFromDirectory(MapperLocalDirectory, 
                        Path.Combine(MapperLocalArchiveDirectory, 
                            $"Mapper_{DateTime.Now:yyyyMMddhhmm}"));
                    Directory.Delete(MapperLocalDirectory, true);
                }

                // Move from inside of the temporary directory into the main mapper folder.
                Directory.Move(mapperTemporaryExtractionSubfolderDirectory, MapperLocalDirectory);
            }
            finally
            {
                CleanupTemporaryFiles();
            }
        }

        public async Task<List<Mapper>> GetMapperList()
        {
            var httpClient = _httpClientFactory.CreateClient();
            if (string.IsNullOrWhiteSpace(_mapperSettingsManager.MapperSettings.MapperDownloadBaseUrl))
            {
                _logger.LogError("The mapper repo url was null or empty.");
                return [];
            }

            try
            {
                //await httpClient.
                return [];
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to retrieve the list of mappers from " +
                                    $"{_mapperSettingsManager.MapperSettings.MapperDownloadBaseUrl}");
                return [];
            }
        }
        public async Task<bool> CheckForUpdates()
        {
            try
            {
                if (BuildEnvironment.IsDebug && _appSettings.MAPPER_DIRECTORY_OVERWRITTEN)
                {
                    _logger.LogWarning("Mapper directory is overwritten, will not perform any updates.");
                    return false;
                }

                if (_mapperSettingsManager.MapperSettings.AlwaysIgnoreUpdates is true)
                {
                    _logger.LogInformation("User requested to ignore updates.");
                    return false;
                }

                if (_mapperSettingsManager.MapperSettings.IgnoreUpdatesUntil is not null &&
                    _mapperSettingsManager.MapperSettings.IgnoreUpdatesUntil > DateTime.Now)
                {
                    return false;
                }
                //`IgnoreUpdatesUntil` timeframe has passed, remove the value
                _mapperSettingsManager.MapperSettings.IgnoreUpdatesUntil = null;
                //Get the list of mappers 
                
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
                if (File.Exists(MapperLocalCommitHashFilePath))
                {
                    localMapperVersion = await File.ReadAllTextAsync(MapperLocalCommitHashFilePath);
                }

                if (_appSettings.MAPPER_VERSION != localMapperVersion)
                {
                    _logger.LogInformation($"Downloading new mappers from server.");

                    var httpClient = _httpClientFactory.CreateClient();

                    await DownloadMappers(httpClient, $"https://github.com/gamehook-io/mappers/archive/{_appSettings.MAPPER_VERSION}.zip");
                    await File.WriteAllTextAsync(MapperLocalCommitHashFilePath, _appSettings.MAPPER_VERSION);

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
    }
}
