

using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Mappers;

namespace PokeAByte.Domain.Services.MapperFile;

public class MapperFileService
{
    private readonly ILogger<MapperFileService> _logger;
    private List<MapperFileData> _installedMappers;
    // private readonly MapperUpdaterSettings _updateSettings;

    public MapperFileService(ILogger<MapperFileService> logger)
    {
        _logger = logger;
        // _updateSettings = MapperUpdaterSettings.Load(_logger);
        _installedMappers = FindInstalled();
    }

    /// <summary>
    /// Deletes target file, if it exists.
    /// </summary>
    /// <param name="path"> The path of the target file. </param>
    private void DeleteFile(string path) {
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


    private void DeleteMapperFromArchive(ArchivedMapperDto archivedMapper)
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

    public void DeleteMappersFromArchive(IEnumerable<ArchivedMapperDto> archivedMappers)
    {
        foreach (var mapper in archivedMappers)
        {
            DeleteMapperFromArchive(mapper);
        }
    }

    public void Refresh()
    {
        _installedMappers = FindInstalled();
    }
}