﻿using GameHook.Domain;
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

    /// <summary>
    /// Archives a file located in filepath into a temp directory located in
    /// archivedPath or MapperEnvironment.MapperArchiveDirectory
    /// </summary>
    /// <param name="relativeFilename">The file name and extension</param>
    /// <param name="filepath">The path where the file is located</param>
    /// <param name="archivedPath">The optional archive path</param>
    /// <returns>The path where the file was archived</returns>
    public string ArchiveFile(string relativeFilename, 
        string filepath, string? archivedPath = null)
    {
        DirectoryInfo? archiveDirectory = null;
        var archivedDirectoryPath = !string.IsNullOrWhiteSpace(archivedPath) ? 
            archivedPath :
            MapperEnvironment.MapperArchiveDirectory;
        //Create a tmp dir to store old mappers
        archiveDirectory = !Directory.Exists(archivedDirectoryPath) ?
            Directory.CreateDirectory(archivedDirectoryPath) : 
            new DirectoryInfo(archivedDirectoryPath);
        
        if (!File.Exists(filepath))
        {
            _logger.LogWarning($"Failed to move {relativeFilename} because it does not exist.\n" +
                               $"\tPath: {filepath}");
            return "";
        }

        //If somehow we fail to create the directory and it doesn't throw an exception, we should still handle it
        if (!archiveDirectory.Exists)
        {
            _logger.LogWarning($"Failed to move {relativeFilename} because " +
                               $"the local archive directory not exist.");
            return "";
        }
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

    public void ArchiveDirectory(string directoryPath, string? archivePath = null)
    {
        var archiveFiles = Directory.GetDirectories(directoryPath).Length != 0 || 
                           Directory.GetFiles(directoryPath).Length != 0;
        
        var archiveDirectoryPath = !string.IsNullOrWhiteSpace(archivePath) ?
            archivePath :
            MapperEnvironment.MapperLocalArchiveDirectory;
        
        if (archiveFiles)
        {
            var archiveDir = Directory.CreateDirectory(archiveDirectoryPath);
            Directory.Move(directoryPath,
                Path.Combine(archiveDir.FullName, $"Mapper_{DateTime.Now:yyyyMMddhhmmss}"));
        }

        try
        {
            //Clean out tmp dir
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
            ArchiveFile(relativePath,mapperPath);
        }
        if (File.Exists(mapperJsPath))
        {
            ArchiveFile(relativePath.Replace(".xml", ".js"), mapperJsPath);
        }
        //Restore the old archived file
        RestoreFile(archivedPath, mapperPath, archiveBasePath);
        RestoreFile(archivedJsPath, mapperJsPath, archiveBasePath);
        /*if(File.Exists(archivedPath))
            File.Move(archivedPath, mapperPath);
        if(File.Exists(archivedJsPath))
            File.Move(archivedJsPath, mapperJsPath);*/
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
}