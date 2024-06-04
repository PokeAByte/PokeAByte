using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Infrastructure.Github;
using Microsoft.Extensions.Logging;

namespace GameHook.Infrastructure.Mappers
{
    public class MapperUpdateManager : IMapperUpdateManager
    {
        private readonly ILogger<MapperUpdateManager> _logger;
        private readonly AppSettings _appSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MapperSettings _mapperSettings;
        private readonly GithubApiSettings _githubApiSettings;
        private readonly GithubRestApi _githubRestApi;

        public MapperUpdateManager(ILogger<MapperUpdateManager> logger, 
            AppSettings appSettings, 
            IHttpClientFactory httpClientFactory,
            MapperSettings mapperSettingsManager, 
            GithubApiSettings githubApiSettings, 
            GithubRestApi githubRestApi)
        {
            _logger = logger;
            _appSettings = appSettings;
            _httpClientFactory = httpClientFactory;
            _mapperSettings = mapperSettingsManager;
            _githubApiSettings = githubApiSettings;
            _githubRestApi = githubRestApi;

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

        //Returns a list of outdated mappers 
        private async Task<List<MapperComparisonDto>> GetOutdatedMapperList()
        {
            //Get the latest version of the mapper tree from github
            var mapperListResponse = await _githubRestApi.GetMapperTreeFile();
            if (mapperListResponse is null || !mapperListResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to download the latest version of the mapper tree json from Github.");
                return [];
            }
            //Convert the remote data to a MapperDto
            var remote = await mapperListResponse.Content.ReadFromJsonAsync<List<MapperDto>>();
            if (remote is null || remote.Count == 0)
            {
                _logger.LogError("Could not read the remote json file.");
                return [];
            }
            //Get the current version the user has on their filesystem
            var mapperTreeUtil = new MapperTreeUtility(MapperLocalDirectory);
            mapperTreeUtil.Load();
            //Compare the mapper trees and return a list of outdated mappers
            return mapperTreeUtil.CompareMapperTrees(remote);

        }
        public async Task<bool> CheckForUpdates()
        {
            try
            {
                if (BuildEnvironment.IsDebug && _appSettings.MAPPER_DIRECTORY_OVERWRITTEN)
                {
                    _logger.LogWarning("Mapper directory is overwritten, will not perform any updates.");
                    _mapperSettings.RequiresUpdate = false;
                    _mapperSettings.SaveChanges(_logger);
                    return false;
                }

                if (_mapperSettings.AlwaysIgnoreUpdates)
                {
                    _logger.LogInformation("User requested to ignore updates.");
                    _mapperSettings.RequiresUpdate = false;
                    _mapperSettings.SaveChanges(_logger);
                    return false;
                }

                if (_mapperSettings.IgnoreUpdatesUntil is not null &&
                    _mapperSettings.IgnoreUpdatesUntil > DateTime.Now)
                {
                    _mapperSettings.RequiresUpdate = false;
                    _mapperSettings.SaveChanges(_logger);
                    return false;
                }
                //`IgnoreUpdatesUntil` timeframe has passed, remove the value
                _mapperSettings.IgnoreUpdatesUntil = null;
                //Get the list of outdated mappers 
                var outdatedMappers = await GetOutdatedMapperList();
                //Save the outdated mapper list
                var jsonData = JsonSerializer.Serialize(outdatedMappers);
                await File.WriteAllTextAsync(BuildEnvironment.OutdatedMapperTreeJson,jsonData);
                _mapperSettings.RequiresUpdate = true;
                _mapperSettings.SaveChanges(_logger);
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

        private void ArchiveFile(string relativeFilename, string filepath, string? archivePath)
        {
            //var filename = relativeFilename[relativeFilename.LastIndexOf('/')..];
            if (!File.Exists(filepath))
            {
                _logger.LogWarning($"Failed to archive {relativeFilename} because it does not exist.\n" +
                                   $"\tPath: {filepath}");
                return;
            }

            if (string.IsNullOrWhiteSpace(archivePath) || !Directory.Exists(archivePath))
            {
                _logger.LogWarning($"Failed to archive {relativeFilename} because " +
                                   $"the archive directory not exist.");
                return;
            }
            try
            {
                var archiveFile = new FileInfo($"{archivePath}/{relativeFilename}");
                archiveFile.Directory?.Create();
                File.Move(filepath, 
                    archiveFile.FullName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to archive {relativeFilename} because of an exception.");
            }
        }

        private void WriteTextToFile(string filepath, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))  
            {
                try
                {
                    var file = new FileInfo(filepath);
                    file.Directory?.Create();
                    File.WriteAllText(file.FullName, text);
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
            DirectoryInfo? archiveDirectory = null;
            //Create a tmp dir to store old mappers
            archiveDirectory = !Directory.Exists(Path.Combine(MapperLocalDirectory, "Archive")) ?
                Directory.CreateDirectory(Path.Combine(MapperLocalDirectory, "Archive")) : 
                new DirectoryInfo(Path.Combine(MapperLocalDirectory, "Archive"));
            foreach (var mapper in updatedMappers)
            {
                var mapperPath = $"{MapperLocalDirectory.Replace("\\", "/")}/{mapper.RelativeXmlPath}";
                var jsPath = $"{MapperLocalDirectory.Replace("\\", "/")}/{mapper.RelativeJsPath}";
                ArchiveFile(mapper.RelativeXmlPath, mapperPath, archiveDirectory?.FullName);
                ArchiveFile(mapper.RelativeJsPath, jsPath, archiveDirectory?.FullName);
                WriteTextToFile(mapperPath, mapper.XmlData);
                WriteTextToFile(jsPath, mapper.JsData);
                /*if(File.Exists(Path.Combine(MapperLocalDirectory.Replace("\\", "/"), mapper.XmlPath)))
                    File.Move(
                        Path.Combine(MapperLocalDirectory.Replace("\\", "/"), mapper.XmlPath),
                        Path.Combine(MapperLocalDirectory.Replace("\\", "/"), "Archive", mapper.XmlPath));
                if(File.Exists(Path.Combine(MapperLocalDirectory.Replace("\\", "/"), mapper.JsPath)))
                    File.Move(
                        Path.Combine(MapperLocalDirectory.Replace("\\", "/"), mapper.JsPath),
                        Path.Combine(MapperLocalDirectory.Replace("\\", "/"), "Archive", mapper.JsPath));*/
                //Write the files to their respective directories
                /*if (!string.IsNullOrEmpty(mapper.XmlData))
                {
                    var file = new
                        FileInfo(MapperLocalDirectory.Replace("\\", "/") + mapper.RelativeXmlPath);
                    file.Directory?.Create();
                    await File.WriteAllTextAsync(file.FullName, mapper.XmlData);
                }

                if (!string.IsNullOrEmpty(mapper.JsData))
                {
                    var file = new
                        FileInfo(MapperLocalDirectory.Replace("\\", "/") + mapper.RelativeJsPath);
                    file.Directory?.Create();
                    await File.WriteAllTextAsync(file.FullName, mapper.JsData);
                }*/
            }

            var archiveFolder = Path.Combine(MapperLocalDirectory, "Archive");
            var archiveFiles = Directory.GetDirectories(archiveFolder).Length != 0 || 
                               Directory.GetFiles(archiveFolder).Length != 0;
            
            //Zip the archived files
            if (archiveFiles)
            {
                var archiveDir = Directory.CreateDirectory(MapperLocalArchiveDirectory);
                /*var archiveMapperDirectory = new DirectoryInfo(Path.Combine(archiveDir.FullName, 
                    $"Mapper_{DateTime.Now:yyyyMMddhhmm}"));
                archiveMapperDirectory.Create();*/
                Directory.Move(Path.Combine(MapperLocalDirectory, "Archive"), 
                    Path.Combine(archiveDir.FullName, $"Mapper_{DateTime.Now:yyyyMMddhhmm}"));
                /*ZipFile.CreateFromDirectory(Path.Combine(MapperLocalDirectory, "Archive"), 
                Path.Combine(MapperLocalArchiveDirectory, 
                    $"Mappers_{DateTime.Now:yyyyMMddhhmm}.zip"));*/
            }

            try
            {
                //Clean out tmp dir
                if (Directory.Exists(Path.Combine(MapperLocalDirectory, "Archive")))
                {
                    Directory.Delete(Path.Combine(MapperLocalDirectory, "Archive"), true);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to remove the archive folder because of an exception.");
            }

            //Update the mapper list
            var mapperTreeUtil = new MapperTreeUtility(MapperLocalDirectory);
            mapperTreeUtil.MapperTree = mapperTreeUtil.GenerateMapperDtoTree();
            mapperTreeUtil.SaveChanges();
            //Finish off by checking for any changes
            await CheckForUpdates();
        }
    }
}