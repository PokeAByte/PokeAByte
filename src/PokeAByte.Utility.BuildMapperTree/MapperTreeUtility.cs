using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using PokeAByte.Domain.Models.Mappers;

namespace PokeAByte.Domain.Services.Mapper;

[JsonSerializable(typeof(List<MapperFile>))]
public partial class MapperTreeUtilityContext : JsonSerializerContext;

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


    private static MapperFile CreateMapperEntry(string baseDirectory, string filepath, string version = "") {
        if (!File.Exists(filepath))
            throw new Exception($"Failed to open file {filepath}.");
        var file = new FileInfo(filepath);
        var path = filepath[baseDirectory.Length..].Replace("\\", "/");
        return new MapperFile(file.Name, path, version);    
    }

    public static List<MapperFile> GenerateMapperDtoTree(string baseDirectory)
    {
        var fileTree = GenerateFileTree(baseDirectory);
        if (fileTree.Count == 0)
            return [];
        return fileTree
            .Select(x => CreateMapperEntry(baseDirectory, x, GetVersion(x)))
            .ToList();
    }


    public static bool SaveChanges(string baseDirectory, List<MapperFile> mapperTree)
    {
        var path = Path.Combine(baseDirectory, "mapper_tree.json");
        if (mapperTree.Count == 0)
        {
            File.WriteAllText(path, "");
            return false;
        }
        var jsonData = JsonSerializer.Serialize(mapperTree, DomainJson.Default.ListMapperFile);
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            Console.WriteLine("Serialized json data is null. Failed to save.");
            return false;
        }
        File.WriteAllText(path, jsonData);
        return true;
    }
}
