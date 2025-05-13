using System.Text.Json;
using System.Xml;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Services.MapperFile;

namespace PokeAByte.Domain.Services.Mapper;

public static class MapperTreeUtility
{
    private static List<string> GenerateFileTree(string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
            throw new Exception("Base directory is null.");
        if (!Directory.Exists(baseDirectory))
        {
            Directory.CreateDirectory(baseDirectory);
        }
        return Directory
            .GetFiles(baseDirectory,
            "*.*",
            SearchOption.AllDirectories)
            .Where(x => x.EndsWith(".xml"))
            .ToList();
    }

    public static string GetVersion(string xmlPath)
    {
        if (!File.Exists(xmlPath))
            return "";
        using var xmlReader = XmlReader.Create(xmlPath);
        xmlReader.MoveToContent();
        var moved = xmlReader.MoveToAttribute("version");
        if (!moved) return "";
        var ver = xmlReader.ReadContentAsString();
        return ver;
    }

    public static List<MapperDto> GenerateMapperDtoTree(string baseDirectory)
    {
        var fileTree = GenerateFileTree(baseDirectory);
        if (fileTree.Count == 0)
            return [];
        return fileTree
            .Select(x => MapperDto.Create(baseDirectory, x, GetVersion(x)))
            .ToList();
    }

    public static List<MapperDto> Load(string baseDirectory)
    {
        var path = Path.Combine(baseDirectory, MapperPaths.MapperTreeJson);
        string? mapperJson = File.Exists(path)
            ? File.ReadAllText(path)
            : null;
        if (string.IsNullOrEmpty(mapperJson))
        {
            var mapperTree = GenerateMapperDtoTree(baseDirectory);
            SaveChanges(baseDirectory, mapperTree);
            return mapperTree;
        }

        try
        {
            var jsonData = File.ReadAllText(path);
            var mapperDtoList = JsonSerializer.Deserialize<List<MapperDto>>(jsonData);
            if (mapperDtoList is not null) return mapperDtoList;
            var mapperTree = GenerateMapperDtoTree(baseDirectory);
            SaveChanges(baseDirectory, mapperTree);
            return mapperTree;
        }
        catch (Exception)
        {
            return GenerateMapperDtoTree(baseDirectory);
        }
    }

    public static bool SaveChanges(string baseDirectory, List<MapperDto> mapperTree)
    {
        var path = Path.Combine(baseDirectory, "mapper_tree.json");
        if (mapperTree.Count == 0)
        {
            File.WriteAllText(path, "");
            return false;
        }
        var jsonData = JsonSerializer.Serialize(mapperTree, MapperDtoContext.Default.ListMapperDto);
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            Console.WriteLine("Serialized json data is null. Failed to save.");
            return false;
        }
        File.WriteAllText(path, jsonData);
        return true;
    }
}

public static class MapperTreeUtilityExtensions
{
    public static List<MapperComparisonDto> CompareMapperTrees(this List<MapperDto> local, List<MapperDto> remoteTree)
    {
        //Get a list of mappers missing from local
        var missing = remoteTree
            .Where(x => local.All(y => y.Path != x.Path))
            .Select(remote => new MapperComparisonDto()
            {
                LatestVersion = remote,
                CurrentVersion = null
            })
            .ToList();
        //We need to create a list of outdated mappers
        //First we need to find the same mapper for both remote and local
        //If the paths are the same then they should be the same mapper
        //Then we run the `Outdated` to compare local and remote's DateUpdatedUtc
        //True means the local mapper is outdated, otherwise it means they aren't outdated
        //or `DateUpdatedUtc` is null
        var outdated = local
            .Where(x => x.Outdated(remoteTree.FirstOrDefault(y => y.Path == x.Path)))
            .Select(localMapper => new MapperComparisonDto()
            {
                CurrentVersion = localMapper,
                LatestVersion = remoteTree.FirstOrDefault(y => y.Path == localMapper.Path)
            })
            .ToList();
        outdated.AddRange(missing);
        return outdated;
    }
}