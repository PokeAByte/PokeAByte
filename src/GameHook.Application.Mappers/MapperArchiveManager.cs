using GameHook.Domain;
using GameHook.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameHook.Mappers;

public class MapperArchiveManager : IMapperArchiveManager
{
    private readonly ILogger<MapperArchiveManager> _logger;

    private Dictionary<string, List<ArchivedMapperDto>> _archivedMappers = [];

    public IReadOnlyDictionary<string, IReadOnlyList<ArchivedMapperDto>> GetArchivedMappers()
        => _archivedMappers.Select(x =>
                new KeyValuePair<string, IReadOnlyList<ArchivedMapperDto>>(x.Key, x.Value.AsReadOnly()))
            .ToDictionary()
            .AsReadOnly();
    
    public MapperArchiveManager(ILogger<MapperArchiveManager> logger)
    {
        _logger = logger;
    }

    private List<KeyValuePair<string, string>> FindAllXmlFiles(string path)
    {
        if (!Directory.Exists(MapperEnvironment.MapperLocalArchiveDirectory))
        {
            _logger.LogWarning($"Failed to find archived files, {MapperEnvironment.MapperLocalArchiveDirectory}" +
                               $" does not exist.");
            return [];
        }

        List<KeyValuePair<string,string>> fileList = [];
        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            try
            {
                if (Directory.GetDirectories(dir).Length > 0)
                    fileList.AddRange(FindAllXmlFiles(dir));
                fileList.AddRange(Directory
                    .EnumerateFiles(dir)
                    .Where(x => x.ToLower().EndsWith(".xml"))
                    .Select(file => new KeyValuePair<string, string>(path, file)));
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
        //get all xml files
        var xmlFiles = FindAllXmlFiles(MapperEnvironment.MapperLocalArchiveDirectory);
        /*var xmlFiles = Directory.GetFiles(MapperEnvironment.MapperLocalArchiveDirectory, 
            "*.xml",
            SearchOption.AllDirectories);*/
        foreach (var xmlFile in xmlFiles)
        {
            //create the mapper dto
            var archivedMapper = MapperDto.Create(MapperEnvironment.MapperLocalArchiveDirectory, 
                xmlFile.Value,
                MapperTreeUtility.GetRevision(xmlFile.Value));
            //get the path's display name
            var pathDisplayName = xmlFile
                .Value[xmlFile.Key.Length..xmlFile.Value.LastIndexOf('\\')]
                .Replace("\\", "/");
            var archivedMapperDto = new ArchivedMapperDto()
            {
                PathDisplayName = pathDisplayName,
                FullPath = xmlFile.Value[..xmlFile.Value.LastIndexOf('\\')],
                Mapper = archivedMapper
            };
            if (_archivedMappers.ContainsKey(xmlFile.Key))
            {
                _archivedMappers.TryGetValue(xmlFile.Key, out var val);
                val?.Add(archivedMapperDto);
            }
            else
            {
                _archivedMappers.TryAdd(xmlFile.Key, [archivedMapperDto]);
            }
        }
    }
    
    public void ArchiveFile(string relativeFilename, 
        string filepath,
        string? archivePath)
    {
        if (!File.Exists(filepath))
        {
            _logger.LogWarning($"Failed to move {relativeFilename} because it does not exist.\n" +
                               $"\tPath: {filepath}");
            return;
        }

        if (string.IsNullOrWhiteSpace(archivePath) || !Directory.Exists(archivePath))
        {
            _logger.LogWarning($"Failed to move {relativeFilename} because " +
                               $"the local archive directory not exist.");
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
            _logger.LogError(e, $"Failed to move {relativeFilename} because of an exception.");
        }
    }

    public void ArchiveDirectory(string directoryPath)
    {
        var archiveFiles = Directory.GetDirectories(directoryPath).Length != 0 || 
                           Directory.GetFiles(directoryPath).Length != 0;
        
        if (archiveFiles)
        {
            var archiveDir = Directory.CreateDirectory(MapperEnvironment.MapperLocalArchiveDirectory);
            Directory.Move(MapperEnvironment.MapperArchiveDirectory,
                Path.Combine(archiveDir.FullName, $"Mapper_{DateTime.Now:yyyyMMddhhmm}"));
        }

        try
        {
            //Clean out tmp dir
            if (Directory.Exists(Path.Combine(MapperEnvironment.MapperLocalDirectory, "Archive")))
            {
                Directory.Delete(Path.Combine(MapperEnvironment.MapperLocalDirectory, "Archive"), true);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to remove the archive folder because of an exception.");
        }

    }
}