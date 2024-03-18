using GameHook.Domain;
using GameHook.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace GameHook.Infrastructure
{
    public class MapperUpdateManager : IMapperUpdateManager
    {
        private readonly ILogger<MapperUpdateManager> _logger;
        private readonly AppSettings _appSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public MapperUpdateManager(ILogger<MapperUpdateManager> logger, AppSettings appSettings, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _appSettings = appSettings;
            _httpClientFactory = httpClientFactory;

            if (Directory.Exists(BuildEnvironment.ConfigurationDirectory) == false)
            {
                _logger.LogInformation("Creating configuration directory for GameHook.");

                Directory.CreateDirectory(BuildEnvironment.ConfigurationDirectory);
            }
        }

        private static string MapperLocalDirectory => Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers");
        private static string MapperLocalCommitHashFilePath => Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers", "COMMIT_HASH.txt");

        private static string MapperTemporaryZipFilepath => Path.Combine(BuildEnvironment.ConfigurationDirectory, $"mappers_tmp.zip");
        private static string MapperTemporaryExtractionDirectory => Path.Combine(BuildEnvironment.ConfigurationDirectory, $"mappers_tmp\\");

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
                File.WriteAllBytes(MapperTemporaryZipFilepath, bytes);

                // Extract to the temporary directory.
                using var zout = ZipFile.OpenRead(MapperTemporaryZipFilepath);
                zout.ExtractToDirectory(MapperTemporaryExtractionDirectory);

                var mapperTemporaryExtractionSubfolderDirectory = Directory.GetDirectories(MapperTemporaryExtractionDirectory).FirstOrDefault() ??
                    throw new Exception("Mappers were downloaded from the server, but did not contain a subfolder.");

                if (Directory.Exists(MapperLocalDirectory))
                {
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

        public async Task<bool> CheckForUpdates()
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
