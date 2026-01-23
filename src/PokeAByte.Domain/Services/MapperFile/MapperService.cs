using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;

internal record AbsoluteMapperPaths(string Xml, string? Script);

internal record MapperFileData(string Path, string XmlContent, string? ScriptContent);

public class MapperService : IMapperService
{
    public static string MapperArchivePath = Path.Combine(BuildEnvironment.ConfigurationDirectory, "MapperArchives");
    public static string MapperDirectory = Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers");
    internal static string MapperTreeFilename = "mapper_tree.json";
    private static string MapperTreePath = Path.Combine(MapperDirectory, MapperTreeFilename);
    private static string RemoteMapperFilePath = Path.Combine(MapperDirectory, "remote_mappers.json");

    private ILogger _logger;

    private IDownloadService _downloadService;
    public List<MapperFile> _managedMappers;
    public List<MapperFile>? _remoteMappers;
    public List<MapperFile> _unmanagedMappers = [];


    public MapperService(ILogger<MapperService> logger, IDownloadService downloadService)
    {
        _logger = logger;
        _downloadService = downloadService;
        InitializeRemoteMappers();
        ReadMappers();
    }

    private AbsoluteMapperPaths GetMapperPaths(string relativePath)
    {
        var xmlPath = Path.Combine(MapperDirectory, relativePath);
        var scriptPath = Path.SetJsExtension(xmlPath);
        if (!File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"File was not found in the the mapper folder.", relativePath);
        }
        if (!xmlPath.EndsWith(".xml"))
        {
            throw new Exception($"Invalid file extension for mapper.");
        }
        if (!File.Exists(scriptPath))
        {
            scriptPath = null;
        }
        return new(xmlPath, scriptPath);
    }

    [MemberNotNull(nameof(_managedMappers), nameof(_unmanagedMappers))]
    private void ReadMappers()
    {
        _managedMappers = (JsonFile.Read(MapperTreePath, DomainJson.Default.ListMapperFile) ?? [])
            // Normalize the mapper path. Previous versions did not remove the leading "/" when saving the .json:
            .Select(x => x with { Path = Path.AsRelative(x.Path) })
            .ToList();

        var removedMappers = _managedMappers.Where(x => !File.Exists(Path.Combine(MapperDirectory, x.Path)));
        if (removedMappers.Count() > 0)
        {
            _logger.LogInformation($"Some mappers could no longer be found in the mapper folder.");
            Save();
        }

        // Find mapper XML files that are not in the _managedMappers list:
        _unmanagedMappers = Directory.GetFiles(MapperDirectory, "*.xml", SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(MapperDirectory, x))              // normalize path
            .Where(path => !_managedMappers.Any(x => x.Path == path))           // check if it's managed
            .Select(path => new MapperFile(Path.GetFileName(path), path, null)) // create model.
            .ToList();

        if (_unmanagedMappers.Any())
        {   
            _logger.LogInformation("Found mapper on disk are not in the mapper_tree.json and can't be updated.");
        }
    }



    private bool CopyMapper(string mapperPath, string sourceDirectory, string targetDirectory, bool overwrite = false)
    {
        var fullTargetPath = Path.Combine(targetDirectory, mapperPath);
        var fullSourcePath = Path.Combine(sourceDirectory, mapperPath);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath)!);
            File.Copy(fullSourcePath, fullTargetPath, overwrite);
            if (File.Exists(Path.SetJsExtension(fullSourcePath)))
            {
                File.Copy(Path.SetJsExtension(fullSourcePath), Path.SetJsExtension(fullTargetPath), overwrite);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                $"Failed to copy mapepr from {fullSourcePath} to {fullTargetPath} because of an exception."
            );
            return false;
        }
    }

    private void InitializeRemoteMappers()
    {
        if (_downloadService.ChangesAvailable())
        {
            _remoteMappers = _downloadService.FetchMapperTree().Result;
        }
        if (_remoteMappers != null)
        {
            JsonFile.Write(_remoteMappers, RemoteMapperFilePath, DomainJson.Default.ListMapperFile);
        }
        else
        {
            _remoteMappers = JsonFile.Read(RemoteMapperFilePath, DomainJson.Default.ListMapperFile);
        }
        if (_remoteMappers == null)
        {
            _logger.LogError("Could not find remote mapper tree data.");
        }
    }

    public bool UpdateRemoteMappers()
    {
        _downloadService.GetLatestUpdate(force: true);
        var response = _downloadService.FetchMapperTree().Result;

        if (response is null)
        {
            _logger.LogError("Failed to download the latest version of the mapper tree json from Github.");
            return false;
        }
        _remoteMappers = response;
        JsonFile.Write(_remoteMappers, RemoteMapperFilePath, DomainJson.Default.ListMapperFile);
        return true;
    }

    private string GetArchiveName() => $"Archive_{DateTime.Now:yyyy-MM-dd_hhmmss}";
    private string GetBackupName() => $"Backup_{DateTime.Now:yyyy-MM-dd_hhmmss}";

    private string? GetMapperVersion(string path)
        => XDocument.Load(path).Element("mapper")?.Attribute("version")?.Value;

    public IEnumerable<InstalledMapper> ListInstalled()
    {
        return [
            .._managedMappers.Select(x => new InstalledMapper(
                x.DisplayName,
                x.Path,
                x.Version,
                MapperFileType.Official
            )),
            .._unmanagedMappers.Select(x => new InstalledMapper(
                x.DisplayName,
                x.Path,
                x.Version,
                MapperFileType.Local
            )),
        ];
    }

    public IEnumerable<RemoteMapperFile> ListRemote()
    {
        return _remoteMappers
            ?.LeftJoin(
                _managedMappers,
                installed => installed.Path,
                remote => remote.Path,
                (a, b) => new { Remote = a, Installed = b }
            )
            ?.Select(x => new RemoteMapperFile(
                x.Remote.DisplayName,
                x.Remote.Path,
                x.Installed?.Version,
                x.Remote.Version
            ))
            ?? [];
    }

    public async Task<MapperContent> LoadContentAsync(string path)
    {
        var absolutePaths = GetMapperPaths(path);
        var mapperContents = await File.ReadAllTextAsync(absolutePaths.Xml);

        return new MapperContent(
            path, 
            mapperContents, 
            absolutePaths.Script, // We don't load the JS content here, because the JavaScript engine takes a path.
            ScriptRoot: MapperDirectory
        );
    }

    public async Task<bool> DownloadAsync(IEnumerable<string> mapperPaths)
    {
        try
        {
            foreach (var path in mapperPaths)
            {
                var remoteMapper = _remoteMappers?.FirstOrDefault(x => x.Path == path);
                if (remoteMapper == null)
                {
                    _logger.LogWarning("Skipped update/download for " + path + " because it's not in the list of remote mappers.");
                    continue;
                }
                var download = await _downloadService.DownloadMapperAsync(path);
                if (download == null)
                {
                    _logger.LogWarning("Download failed for mapper " + path);
                    continue;
                }
                var xmlPath = Path.Combine(MapperDirectory, download.RelativeXmlPath);
                Directory.CreateDirectory(Path.GetDirectoryName(xmlPath)!);
                await File.WriteAllTextAsync(xmlPath, download.XmlData);
                if (!string.IsNullOrEmpty(download.JsData))
                {
                    await File.WriteAllTextAsync(Path.Combine(MapperDirectory, download.RelativeJsPath), download.JsData);
                }
                var installedMapper = _managedMappers.FirstOrDefault(x => x.Path == download.RelativeXmlPath);
                if (installedMapper != null)
                {
                    _managedMappers.Remove(installedMapper);
                }
                _managedMappers.Add(remoteMapper);
                Save();
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download of one or more mappers failed because of an unhandled exception.");
            return false;
        }
    }

    private void Save()
    {
        JsonFile.Write(_managedMappers, MapperTreePath, DomainJson.Default.ListMapperFile);
    }

    public List<ArchivedMapperFile> ListArchived()
    {
        if (!Directory.Exists(MapperArchivePath))
        {
            return [];
        }
        var result = new List<ArchivedMapperFile>();
        foreach (var archivePath in Directory.GetDirectories(MapperArchivePath))
        {
            foreach (var mapperPath in Directory.GetFiles(archivePath, "*.xml", SearchOption.AllDirectories))
            {
                result.Add(new ArchivedMapperFile(
                    Path: Path.GetRelativePath(MapperArchivePath, archivePath),
                    Mapper: new MapperFile(
                        Path.GetFileName(mapperPath),
                        Path.GetRelativePath(archivePath, mapperPath),
                        GetMapperVersion(mapperPath)
                    )
                ));
            }
        }
        return result;
    }

    public bool Restore(string archivePath)
    {
        var archiveDirectory = Path.Combine(MapperArchivePath, archivePath);
        var archiveFiles = Directory.GetFiles(archiveDirectory, "*xml", SearchOption.AllDirectories);
        var restoreFiles = new List<MapperFileData>();
        // Move existing mappers into a new archive:
        var conflictArchivePath = Path.Combine(MapperArchivePath, GetArchiveName());
        foreach (var archivedMapper in archiveFiles.Select(x => Path.GetRelativePath(archiveDirectory, x)))
        {
            var currentInstalledPath = Path.Combine(MapperDirectory, archiveDirectory);
            if (File.Exists(currentInstalledPath))
            {
                if (!CopyMapper(archivedMapper, MapperDirectory, conflictArchivePath))
                {
                    _logger.LogError("Could not archive mapper " + archivedMapper + ". Aborting archive restoration.");
                    return false;
                }
            }
        }
        foreach (var archivedMapper in archiveFiles.Select(x => Path.GetRelativePath(archiveDirectory, x)))
        {
            if (!CopyMapper(archivedMapper, archiveDirectory, MapperDirectory))
            {
                _logger.LogError($"Could not restore mapper {archivedMapper} from archive.");
                return false;
            }
            _managedMappers.RemoveAll(x => x.Path == archivedMapper);
            _managedMappers.Add(new MapperFile(
                Path.GetFileName(archivedMapper),
                archivedMapper,
                GetMapperVersion(Path.Combine(MapperDirectory, archivedMapper))
            ));
        }
        Directory.Delete(archiveDirectory, recursive: true);
        Save();
        return true;
    }

    public void DeleteArchive(string archivePath)
    {
        Directory.Delete(Path.Combine(MapperArchivePath, archivePath), recursive: true);
    }

    public async Task<bool> Backup(IEnumerable<string> paths)
    {
        string destinationFolder = Path.Combine(MapperArchivePath, GetBackupName());
        foreach (var path in paths)
        {
            if (!CopyMapper(path, Path.Combine(MapperDirectory), destinationFolder))
            {
                _logger.LogError($"Failed to backup {path}.");
                return false;
            }
        }
        return true; ;
    }

    public bool Archive(IEnumerable<string> paths)
    {
        string destinationFolder = Path.Combine(MapperArchivePath, GetArchiveName());
        foreach (var path in paths)
        {
            if (!CopyMapper(path, Path.Combine(MapperDirectory), destinationFolder))
            {
                _logger.LogError($"Failed to archive {path}. Original files will be retained");
                return false;
            }
        }
        foreach (var path in paths)
        {
            var fullPath = Path.Combine(MapperDirectory, Path.AsRelative(path));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            if (File.Exists(Path.SetJsExtension(fullPath)))
            {
                File.Delete(Path.SetJsExtension(fullPath));
            }
            _managedMappers.RemoveAll(x => x.Path == path);
        }
        Save();
        return true;
    }
}
