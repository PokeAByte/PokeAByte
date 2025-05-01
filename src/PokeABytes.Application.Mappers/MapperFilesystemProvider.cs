using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Application.Mappers;

public class MapperFilesystemProvider : IMapperFilesystemProvider
{
    private readonly AppSettings _appSettings;

    public IEnumerable<MapperFilesystemDTO> MapperFiles { get; private set; } = new List<MapperFilesystemDTO>();

    public MapperFilesystemProvider(AppSettings appSettings)
    {
        _appSettings = appSettings;
        CacheMapperFiles();
    }

    public void CacheMapperFiles()
    {
        MapperFiles = GetAllMapperFiles();
    }

    private string GetId(MapperFilesystemTypes type, string filePath)
    {
        //Not sure why we should throw an exception instead of just returning string.Empty? 
        if (filePath.Contains(".."))
        {
            throw new Exception("Invalid characters in file path.");
        }

        var pathParts = filePath.Split(Path.DirectorySeparatorChar);

        var directory = pathParts.Length > 1 ? pathParts[pathParts.Length - 2] : "";
        var filenameWithExtension = pathParts[pathParts.Length - 1];

        var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filenameWithExtension);

        var formattedFilename = filenameWithoutExtension.Replace(' ', '_');

        return $"{type}_{directory}_{formattedFilename}".ToLower();
    }

    private string GetDisplayName(string filePath)
    {
        //Not sure why we should throw an exception instead of just returning string.Empty? 
        if (filePath.Contains(".."))
        {
            throw new Exception("Invalid characters in file path.");
        }

        var pathParts = filePath.Split(Path.DirectorySeparatorChar);

        var directory = pathParts.Length > 1 ? pathParts[pathParts.Length - 2] : "";
        var filenameWithExtension = pathParts[pathParts.Length - 1];

        var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filenameWithExtension);

        var formattedFilename = filenameWithoutExtension.Replace('_', ' ');

        return $"({directory.ToUpper()}) {formattedFilename}";
    }

    /// <summary>
    /// We replace the base path with an empty string
    /// as to not expose the absolute path of the filesystem.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<MapperFilesystemDTO> GetAllMapperFiles()
    {
        if (_appSettings.MAPPER_DIRECTORY.Contains(".."))
        {
            throw new Exception("Invalid characters in mapper folder path.");
        }
        //It is possible that the mapper dir is missing, if so this will cause a lot of issues.
        if (!Directory.Exists(_appSettings.MAPPER_DIRECTORY))
        {
            //Just create the directory again
            Directory.CreateDirectory(_appSettings.MAPPER_DIRECTORY);
        }
        var mappers = new DirectoryInfo(_appSettings.MAPPER_DIRECTORY)
            .GetFiles("*.xml", SearchOption.AllDirectories)
            .Select(x => new MapperFilesystemDTO()
            {
                Id = GetId(MapperFilesystemTypes.Official, x.FullName),
                Type = MapperFilesystemTypes.Official,
                AbsolutePath = x.FullName,
                DisplayName = $"{GetDisplayName(x.FullName)}"
            })
            .ToList();

        if (_appSettings.MAPPER_LOCAL_DIRECTORY != null)
        {
            //I am not sure why this is required? I will comment this out for now since this causes
            //issues with the debugging process if we have mappers within the application build folder.
            //Maybe it would be a better idea to just return what we have instead of throwing an exception and
            //breaking everything?
            if (/*_appSettings.MAPPER_LOCAL_DIRECTORY.Contains('.') ||*/
                _appSettings.MAPPER_LOCAL_DIRECTORY.Contains(".."))
            {
                throw new Exception("Invalid characters in mapper folder path.");
            }

            var localMappers = new DirectoryInfo(_appSettings.MAPPER_LOCAL_DIRECTORY)
                .GetFiles("*.xml", SearchOption.AllDirectories)
                .Select(x => new MapperFilesystemDTO()
                {
                    Id = GetId(MapperFilesystemTypes.Local, x.FullName),
                    Type = MapperFilesystemTypes.Local,
                    AbsolutePath = x.FullName,
                    DisplayName = $"(Local) {GetDisplayName(x.FullName)}"
                })
                .ToList();

            mappers.AddRange(localMappers);
        }

        return mappers;
    }

    public string GetMapperRootDirectory(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath))
        {
            throw new Exception("Absolute path was not supplied.");
        }

        var path = absolutePath;

        while (path != null)
        {
            if (path.ToLower().EndsWith($"{Path.DirectorySeparatorChar}mappers"))
            {
                return path;
            }

            if (path.EndsWith(":\\") || path == "/")
            {
                throw new Exception($"Could not find a mappers directory in function GetMapperRootDirectory() with an absolute path of {absolutePath}. Reached the filesystem root.");
            }

            path = Path.GetDirectoryName(path);
        }

        throw new Exception($"Could not find a mappers directory in function GetMapperRootDirectory() with an absolute path of {absolutePath}.");
    }

    public string GetRelativePath(string absolutePath)
    {
        if (absolutePath.Contains(".."))
        {
            throw new Exception("The path provided does not appear to be an absolute path.");
        }

        if (absolutePath.StartsWith(_appSettings.MAPPER_DIRECTORY))
        {
            return string.Concat(".", absolutePath.AsSpan(_appSettings.MAPPER_DIRECTORY.Length));
        }
        else if (_appSettings.MAPPER_LOCAL_DIRECTORY != null && absolutePath.StartsWith(_appSettings.MAPPER_LOCAL_DIRECTORY))
        {
            return string.Concat(".", absolutePath.AsSpan(_appSettings.MAPPER_LOCAL_DIRECTORY.Length));
        }
        else
        {
            throw new Exception($"The absolute path {absolutePath} does not appear to be a mapper directory.");
        }
    }

    public async Task<MapperContent> LoadContentAsync(string mapperId)
    {
         // Get the file path from the filesystem provider.
        var mapperFile = MapperFiles.SingleOrDefault(x => x.Id == mapperId) ??
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
            scriptRoot = GetMapperRootDirectory(javascriptAbsolutePath);
            scriptPath = GetRelativePath(javascriptAbsolutePath);
        }
        return new MapperContent(mapperContents, scriptPath, scriptRoot);
    }
}
