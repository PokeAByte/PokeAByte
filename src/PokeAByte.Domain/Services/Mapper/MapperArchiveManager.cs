using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Services.MapperFile;

namespace PokeAByte.Domain.Services.Mapper;

public class MapperArchiveManager : IMapperArchiveManager
{
    private readonly ILogger<MapperArchiveManager> _logger;

    public MapperArchiveManager(ILogger<MapperArchiveManager> logger)
    {
        _logger = logger;
    }

    private DirectoryInfo? CreateArchiveDirectory(string relativeFilename)
    {
        DirectoryInfo? archiveDirectory = Directory.CreateDirectory(MapperPaths.MapperArchiveDirectory);
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
            File.Move(filepath, archiveFile.FullName);
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

    public void ArchiveDirectory(string directoryPath)
    {
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
        ArchiveDirectory(MapperPaths.MapperArchiveDirectory);
    }

    private void RestoreMapperFromArchive(ArchivedMapperDto archivedMapper)
    {
        //Locate the original file
        //Build the paths
        var relativePath = $"{archivedMapper.PathDisplayName}/{archivedMapper.Mapper.DisplayName}";
        var mapperPath = $"{MapperPaths.MapperDirectory}/{relativePath}";
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
}