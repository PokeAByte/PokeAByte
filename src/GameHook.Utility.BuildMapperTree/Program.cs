using System.Text.Json.Serialization;
using GameHook.Mappers;

namespace GameHook.Utility.BuildMapperTree;

class Program
{
    static void Main(string[] args)
    {
        if(args.Length == 0)
            Console.WriteLine("Please provide the mapper directory or drag-and-drop the mapper directory onto the exe.");
        var mapperTree = MapperTreeUtility.GenerateMapperDtoTree(args.First());
        var saved = MapperTreeUtility.SaveChanges(args.First(), mapperTree);
        if (saved is false)
            Console.WriteLine("Failed to save file.");
        else
            Console.WriteLine("Saved file.");
        Console.ReadKey();
    }
}

