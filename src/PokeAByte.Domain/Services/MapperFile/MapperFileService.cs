using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Services.Mapper;

namespace PokeAByte.Domain.Services.MapperFile;

public class MapperFileService : IMapperFileService
{
    private readonly ILogger<IMapperFileService> _logger;
    private List<MapperFileData> _installedMappers;
    // private readonly MapperUpdaterSettings _updateSettings;

    public MapperFileService(ILogger<IMapperFileService> logger)
    {
        _logger = logger;
        // _updateSettings = MapperUpdaterSettings.Load(_logger);
        _installedMappers = FindInstalled();
    }

    private DirectoryInfo? CreateArchiveDirectory(string relativeFilename)
    {
        DirectoryInfo? archiveDirectory = Directory.CreateDirectory(MapperPaths.MapperArchiveDirectory);
        if (archiveDirectory.Exists) return archiveDirectory;

        //If somehow we fail to create the directory and it doesn't throw an exception, we should still handle it
        _logger.LogWarning($"Failed to move {relativeFilename} because the local archive directory not exist.");
        return null;
    }

    private void RestoreArchivedFile(string sourceFile, string destinationFile, string sourcePath)
    {
        var destiniationFolder = Path.GetDirectoryName(destinationFile);
        if (!File.Exists(sourceFile) || destiniationFolder == null) return;
        if (File.Exists(destinationFile))
        {
            _logger.LogWarning($"{destinationFile} already exists.");
            return;
        }
        Directory.CreateDirectory(destiniationFolder);
        File.Move(sourceFile, destinationFile);
        if (Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories).Length == 0)
        {
            Directory.Delete(sourcePath, true);
        }
    }

    /// <summary>
    /// Deletes target file, if it exists.
    /// </summary>
    /// <param name="path"> The path of the target file. </param>
    private void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string GetDisplayName(string filePath)
    {
        var directory = Directory.GetParent(filePath)?.Name ?? "";
        var fileName = Path.GetFileNameWithoutExtension(filePath).Replace('_', ' ');
        return $"({directory.ToUpper()}) {fileName}";
    }

    private string GetId(MapperFilesystemTypes type, string filePath)
    {
        var directory = Directory.GetParent(filePath)?.Name ?? "";
        var filename = Path.GetFileNameWithoutExtension(filePath).Replace(' ', '_');
        return $"{type}_{directory}_{filename}".ToLower();
    }

    private List<MapperFileData> FindInstalled()
    {
        if (!Directory.Exists(MapperPaths.MapperDirectory))
        {
            Directory.CreateDirectory(MapperPaths.MapperDirectory);
        }
        var mappers = new DirectoryInfo(MapperPaths.MapperDirectory)
            .GetFiles("*.xml", SearchOption.AllDirectories)
            .Select(x => new MapperFileData(
                    GetId(MapperFilesystemTypes.Official, x.FullName),
                    MapperFilesystemTypes.Official,
                    x.FullName,
                    $"{GetDisplayName(x.FullName)}"
                )
            )
            .ToList();
        var localDirectory = MapperPaths.LocalMapperDirectory;
        if (localDirectory != null && Path.Exists(localDirectory))
        {
            var localMappers = new DirectoryInfo(localDirectory)
                .GetFiles("*.xml", SearchOption.AllDirectories)
                .Select(x => new MapperFileData(
                    GetId(MapperFilesystemTypes.Local, x.FullName),
                    MapperFilesystemTypes.Local,
                    x.FullName,
                    $"(Local) {GetDisplayName(x.FullName)}")
                );
            mappers.AddRange(localMappers);
        }

        return mappers;
    }
    public List<MapperFileData> ListInstalled() => _installedMappers;

    public List<ArchivedMapperDto> ListArchived()
    {
        if (!Directory.Exists(MapperPaths.MapperLocalArchiveDirectory))
        {
            return [];
        }
        var result = new List<ArchivedMapperDto>();
        foreach (var archivePath in Directory.GetDirectories(MapperPaths.MapperLocalArchiveDirectory))
        {
            foreach (var mapperPath in Directory.GetFiles(archivePath, "*.xml", SearchOption.AllDirectories))
            {
                result.Add(new ArchivedMapperDto()
                {
                    PathDisplayName = Path.GetRelativePath(archivePath, Path.GetDirectoryName(mapperPath) ?? ""),
                    FullPath = Path.GetDirectoryName(mapperPath) ?? "",
                    Mapper = MapperDto.Create(
                        Path.GetRelativePath(archivePath, mapperPath),
                        mapperPath,
                        "0.0"
                    )
                }
                );
            }
        }
        return result;
    }

    public async Task<MapperContent> LoadContentAsync(string mapperId)
    {
        // Get the file path from the filesystem provider.
        var mapperFile = ListInstalled().SingleOrDefault(x => x.Id == mapperId) ??
            throw new Exception($"Unable to determine a mapper with the ID of {mapperId}.");

        if (File.Exists(mapperFile.AbsolutePath) == false)
        {
            throw new FileNotFoundException($"File was not found in the {mapperFile.Type} mapper folder.", mapperFile.DisplayName);
        }
        var mapperContents = await File.ReadAllTextAsync(mapperFile.AbsolutePath);
        if (!mapperFile.AbsolutePath.EndsWith(".xml"))
        {
            throw new Exception($"Invalid file extension for mapper.");
        }
        string? scriptRoot = null;
        string? scriptPath = null;
        var javascriptAbsolutePath = mapperFile.AbsolutePath.Replace(".xml", ".js");
        if (File.Exists(javascriptAbsolutePath))
        {
            scriptRoot = MapperPaths.MapperDirectory;
            scriptPath = "./" + Path.GetRelativePath(MapperPaths.MapperDirectory, javascriptAbsolutePath);
        }
        return new MapperContent(mapperContents, scriptPath, scriptRoot);
    }

    /// <summary>
    /// Updates the mapper tree (see <see cref="MapperTreeUtility.SaveChanges"/>) and the internally cached list
    /// of available / installed mappers.
    /// </summary>
    public void Refresh()
    {
        var mapperTree = MapperTreeUtility.GenerateMapperDtoTree(MapperPaths.MapperDirectory);
        MapperTreeUtility.SaveChanges(MapperPaths.MapperDirectory, mapperTree);
        _installedMappers = FindInstalled();
    }

    /// <summary>
    /// Archives a file located in filepath into a temp directory located in MapperEnvironment.MapperArchiveDirectory
    /// </summary>
    /// <param name="relativeFilename">The file name and extension</param>
    /// <param name="filepath">The path where the file is located</param>
    /// <param name="archivedPath">The optional archive path</param>
    /// <returns>The path where the file was archived</returns>
    public void ArchiveFile(string relativeFilename, string filepath)
    {
        if (!File.Exists(filepath))
        {
            _logger.LogWarning($"Failed to move {relativeFilename} because it does not exist.\n\tPath: {filepath}");
            return;
        }
        var archiveDirectory = CreateArchiveDirectory(relativeFilename);
        if (archiveDirectory is null) return;
        try
        {
            var archiveFile = new FileInfo($"{archiveDirectory.FullName}/{relativeFilename}");
            archiveFile.Directory?.Create();
            File.Move(filepath, archiveFile.FullName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to move {relativeFilename} because of an exception.");
        }
    }

    public void ArchiveDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }
        var archiveFiles = Directory.GetDirectories(directoryPath).Length != 0
            || Directory.GetFiles(directoryPath).Length != 0;

        var archiveDirectory = MapperPaths.MapperLocalArchiveDirectory;

        if (archiveFiles)
        {
            var archiveDir = Directory.CreateDirectory(archiveDirectory);
            Directory.Move(directoryPath, Path.Combine(archiveDir.FullName, $"Mapper_{DateTime.Now:yyyyMMddhhmmss}"));
        }

        try
        {
            // Clean out tmp dir
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to remove the archive folder because of an exception.");
        }
    }

    public void RestoreMappersFromArchive(List<ArchivedMapperDto> archivedMappers)
    {
        if (archivedMappers.Count == 0)
            return;
        foreach (var archivedMapper in archivedMappers)
        {
            // Locate the original file
            // Build the paths
            var relativePath = $"{archivedMapper.PathDisplayName}/{archivedMapper.Mapper.DisplayName}";
            var mapperPath = $"{MapperPaths.MapperDirectory}/{relativePath}";
            var mapperJsPath = mapperPath.Replace(".xml", ".js");
            var archivedPath = $"{archivedMapper.FullPath}/{archivedMapper.Mapper.DisplayName}";
            var archivedJsPath = archivedPath.Replace(".xml", ".js");
            var archiveBasePath = archivedMapper.FullPath[..^archivedMapper.PathDisplayName.Length];
            // If the original file exists, back it up
            if (File.Exists(mapperPath))
            {
                // Archive the original in a tmp dir
                ArchiveFile(relativePath, mapperPath);
            }
            if (File.Exists(mapperJsPath))
            {
                ArchiveFile(relativePath.Replace(".xml", ".js"), mapperJsPath);
            }
            // Restore the old archived file
            RestoreArchivedFile(archivedPath, mapperPath, archiveBasePath);
            RestoreArchivedFile(archivedJsPath, mapperJsPath, archiveBasePath);
        }
        // Finish archiving the original files
        ArchiveDirectory(MapperPaths.MapperArchiveDirectory);
    }

    public void DeleteMappersFromArchive(IEnumerable<ArchivedMapperDto> archivedMappers)
    {
        foreach (var archivedMapper in archivedMappers)
        {
            var xmlPath = Path.Combine(archivedMapper.FullPath, archivedMapper.Mapper.DisplayName);
            var scriptPath = xmlPath.Replace(".xml", ".js");
            DeleteFile(xmlPath);
            DeleteFile(scriptPath);
            var archiveDir = Path.GetDirectoryName(archivedMapper.FullPath) ?? "";
            if (Directory.GetFiles(archiveDir, "*.*", SearchOption.AllDirectories).Length == 0)
            {
                Directory.Delete(archiveDir, true);
            }
        }
    }

    public string BackupFile(string relativeFilename, string filepath)
    {
        if (!File.Exists(filepath))
        {
            _logger.LogWarning($"Failed to move {relativeFilename} because it does not exist.\n\tPath: {filepath}");
            return "";
        }
        var archiveDirectory = CreateArchiveDirectory(relativeFilename);
        if (archiveDirectory is null) return "";
        try
        {
            var archiveFile = new FileInfo($"{archiveDirectory.FullName}/{relativeFilename}");
            archiveFile.Directory?.Create();
            File.Copy(filepath, archiveFile.FullName);
            return archiveDirectory.FullName;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to copy {relativeFilename} because of an exception.");
            return "";
        }
    }
}