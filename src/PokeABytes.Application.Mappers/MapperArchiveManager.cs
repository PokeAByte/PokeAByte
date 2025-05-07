using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Mappers;

namespace PokeAByte.Application.Mappers;

public class MapperArchiveManager : IMapperArchiveManager
{
    private readonly ILogger<MapperArchiveManager> _logger;

    private Dictionary<string, List<ArchivedMapperDto>> _archivedMappers = [];

    public IReadOnlyDictionary<string, IReadOnlyList<ArchivedMapperDto>> GetArchivedMappers()
        => _archivedMappers.Select(
                x => new KeyValuePair<string, IReadOnlyList<ArchivedMapperDto>>(x.Key, x.Value.AsReadOnly())
            )
            .ToDictionary()
            .AsReadOnly();

    public MapperArchiveManager(ILogger<MapperArchiveManager> logger)
    {
        _logger = logger;
        GenerateArchivedList();
    }

    private List<MapperXmlFileDto> FindAllXmlFiles(string path, string basePath = "")
    {
        if (!Directory.Exists(MapperEnvironment.MapperLocalArchiveDirectory))
        {
            _logger.LogWarning($"Failed to find archived files, {MapperEnvironment.MapperLocalArchiveDirectory} does not exist.");
            return [];
        }

        List<MapperXmlFileDto> fileList = [];
        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            if (string.IsNullOrWhiteSpace(basePath))
                basePath = dir;
            try
            {
                if (Directory.GetDirectories(dir).Length > 0)
                    fileList.AddRange(FindAllXmlFiles(dir, basePath));
                fileList.AddRange(Directory
                    .EnumerateFiles(dir)
                    .Where(x => x.ToLower().EndsWith(".xml"))
                    .Select(file => MapperXmlFileDto.Create(file, basePath)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to access files in ${dir} due to an exception.");
            }
        }
        return fileList;
    }

    public void GenerateArchivedList()
    {
        _archivedMappers = [];
        //get all xml files
        var xmlFiles = FindAllXmlFiles(MapperEnvironment.MapperLocalArchiveDirectory);
        /*var xmlFiles = Directory.GetFiles(MapperEnvironment.MapperLocalArchiveDirectory, 
            "*.xml",
            SearchOption.AllDirectories);*/
        foreach (var xmlFile in xmlFiles)
        {
            //create the mapper dto
            var archivedMapper = MapperDto.Create(MapperEnvironment.MapperLocalArchiveDirectory,
                xmlFile.FilePath,
                MapperTreeUtility.GetVersion(xmlFile.FilePath));
            //get the path's display name
            var archivedMapperDto = new ArchivedMapperDto()
            {
                PathDisplayName = xmlFile.RelativePath,
                FullPath = xmlFile.FullPath,
                Mapper = archivedMapper
            };
            if (_archivedMappers.ContainsKey(xmlFile.FullPath))
            {
                _archivedMappers.TryGetValue(xmlFile.FullPath, out var val);
                val?.Add(archivedMapperDto);
            }
            else
            {
                _archivedMappers.TryAdd(xmlFile.FullPath, [archivedMapperDto]);
            }
        }
    }

    private DirectoryInfo? CreateArchiveDirectory(string relativeFilename, string filepath)
    {
        //Create a tmp dir to store old mappers
        DirectoryInfo? archiveDirectory = !Directory.Exists(MapperEnvironment.MapperArchiveDirectory) 
            ? Directory.CreateDirectory(MapperEnvironment.MapperArchiveDirectory) 
            : new DirectoryInfo(MapperEnvironment.MapperArchiveDirectory);

        if (!File.Exists(filepath))
        {
            _logger.LogWarning($"Failed to move {relativeFilename} because it does not exist.\n\tPath: {filepath}");
            return null;
        }

        if (archiveDirectory.Exists) return archiveDirectory;

        //If somehow we fail to create the directory and it doesn't throw an exception, we should still handle it
        _logger.LogWarning($"Failed to move {relativeFilename} because the local archive directory not exist.");
        return null;
    }

    /// <summary>
    /// Archives a file located in filepath into a temp directory located in
    /// archivedPath or MapperEnvironment.MapperArchiveDirectory
    /// </summary>
    /// <param name="relativeFilename">The file name and extension</param>
    /// <param name="filepath">The path where the file is located</param>
    /// <param name="archivedPath">The optional archive path</param>
    /// <returns>The path where the file was archived</returns>
    public string ArchiveFile(string relativeFilename, string filepath)
    {
        var archiveDirectory = CreateArchiveDirectory(relativeFilename, filepath);
        if (archiveDirectory is null) return "";
        try
        {
            var archiveFile = new FileInfo($"{archiveDirectory.FullName}/{relativeFilename}");
            archiveFile.Directory?.Create();
            File.Move(filepath,
                archiveFile.FullName);
            return archiveDirectory.FullName;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to move {relativeFilename} because of an exception.");
            return "";
        }
    }

    public string BackupFile(string relativeFilename, string filepath)
    {
        var archiveDirectory = CreateArchiveDirectory(relativeFilename, filepath);
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

    public void ArchiveDirectory(string directoryPath)
    {
        var archiveFiles = Directory.GetDirectories(directoryPath).Length != 0 
            || Directory.GetFiles(directoryPath).Length != 0;

        var archiveDirectory = MapperEnvironment.MapperLocalArchiveDirectory;

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
                Directory.Delete(directoryPath, true);
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
        foreach (var mapper in archivedMappers)
        {
            RestoreMapperFromArchive(mapper);
        }
        //Finish archiving the original files
        ArchiveDirectory(MapperEnvironment.MapperArchiveDirectory);
        //Regenerate the archive list
        GenerateArchivedList();
    }

    private void RestoreMapperFromArchive(ArchivedMapperDto archivedMapper)
    {
        //Locate the original file
        //Build the paths
        var relativePath = $"{archivedMapper.PathDisplayName}/{archivedMapper.Mapper.DisplayName}";
        var mapperPath = $"{MapperEnvironment.MapperLocalDirectory}/{relativePath}";
        var mapperJsPath = mapperPath.Replace(".xml", ".js");
        var archivedPath = $"{archivedMapper.FullPath}/{archivedMapper.Mapper.DisplayName}";
        var archivedJsPath = archivedPath.Replace(".xml", ".js");
        var archiveBasePath =
            archivedMapper.FullPath[..^archivedMapper.PathDisplayName.Length];
        //If the original file exists, back it up
        if (File.Exists(mapperPath))
        {
            //Archive the original in a tmp dir
            ArchiveFile(relativePath, mapperPath);
        }
        if (File.Exists(mapperJsPath))
        {
            ArchiveFile(relativePath.Replace(".xml", ".js"), mapperJsPath);
        }
        //Restore the old archived file
        RestoreFile(archivedPath, mapperPath, archiveBasePath);
        RestoreFile(archivedJsPath, mapperJsPath, archiveBasePath);
    }

    private void RestoreFile(string sourceFile, string destinationFile, string sourcePath)
    {
        if (!File.Exists(sourceFile)) return;
        if (File.Exists(destinationFile))
        {
            _logger.LogWarning($"{destinationFile} already exists.");
            return;
        }
        File.Move(sourceFile, destinationFile);
        if (Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories).Length == 0)
        {
            Directory.Delete(sourcePath, true);
        }
    }

    public void DeleteMappersFromArchive(List<ArchivedMapperDto> archivedMappers)
    {
        if (archivedMappers.Count == 0)
            return;
        foreach (var mapper in archivedMappers)
        {
            DeleteMapperFromArchive(mapper);
        }
        GenerateArchivedList();
    }

    private void DeleteMapperFromArchive(ArchivedMapperDto archivedMapper)
    {
        var absolutePath = Path.Combine(archivedMapper.FullPath, archivedMapper.Mapper.DisplayName);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
        if (File.Exists(absolutePath.Replace(".xml", ".js")))
        {
            File.Delete(absolutePath.Replace(".xml", ".js"));
        }

        var archiveDir = archivedMapper.FullPath[..^archivedMapper.PathDisplayName.Length];
        if (Directory.GetFiles(archiveDir, "*.*", SearchOption.AllDirectories).Length == 0)
        {
            Directory.Delete(archiveDir, true);
        }
    }
}