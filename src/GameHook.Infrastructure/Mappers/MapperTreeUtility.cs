using System.Text.Json;
using System.Xml;
using GameHook.Domain;

namespace GameHook.Infrastructure.Mappers;

public class MapperTreeUtility(string baseDirectory)
{
    public readonly string BaseDirectory = baseDirectory;
    //public List<string> FileTree = [];
    public List<MapperDto> MapperTree = [];
    private List<string> GenerateFileTree()
    {
        if (string.IsNullOrWhiteSpace(BaseDirectory))
            throw new Exception("Base directory is null.");
        if (!Directory.Exists(BaseDirectory))
        {
            Directory.CreateDirectory(BaseDirectory);
        }
        return Directory
            .GetFiles(BaseDirectory, 
            "*.*", 
            SearchOption.AllDirectories)
            .Where(x => x.EndsWith(".xml"))
            .ToList();
    }

    private int GetRevision(string xmlPath)
    {
        if (!File.Exists(xmlPath))
            return 0;
        using var xmlReader = XmlReader.Create(xmlPath);
        xmlReader.MoveToContent();
        var moved = xmlReader.MoveToAttribute("revision");
        if (!moved) return 0;
        var rev = xmlReader.ReadContentAsString();
        int.TryParse(rev, out var revision);
        return revision;
    }
    public List<MapperDto> GenerateMapperDtoTree()
    {
        var fileTree = GenerateFileTree();
        if (fileTree.Count == 0)
            return [];
        return fileTree
            .Select(x => MapperDto.Create(BaseDirectory, x, GetRevision(x)))
            .ToList();
    }
    public void Load()
    {
        var path = Path.Combine(BaseDirectory, "mapper_tree.json");
        if (!File.Exists(path))
        {
            Console.WriteLine($"{path} does not exist.");
            MapperTree = GenerateMapperDtoTree();
            SaveChanges();
            return;
        }

        try
        {
            var jsonData = File.ReadAllText(path);
            var mapperDtoList = JsonSerializer.Deserialize<List<MapperDto>>(jsonData);
            if (mapperDtoList is null)
            {
                MapperTree = GenerateMapperDtoTree();
                SaveChanges();
            }
            else
            {
                MapperTree = mapperDtoList;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            MapperTree = GenerateMapperDtoTree();
        }
    }
    public bool SaveChanges()
    {
        if (MapperTree.Count == 0)
        {
            Console.WriteLine("Mapper tree is empty. Failed to save.");
            return false;
        }
        var path = Path.Combine(BaseDirectory, "mapper_tree.json");
        var jsonData = JsonSerializer.Serialize(MapperTree);
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
    public static List<MapperComparisonDto> CompareMapperTrees(this MapperTreeUtility local, List<MapperDto> remoteTree)
    {
        //Get a list of mappers missing from local
        var missing = remoteTree
            .Where(x => local.MapperTree.All(y => y.Path != x.Path))
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
        var outdated = local.MapperTree.Where(x =>
                x.Outdated(remoteTree
                    .FirstOrDefault(y =>
                        y.Path == x.Path)))
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