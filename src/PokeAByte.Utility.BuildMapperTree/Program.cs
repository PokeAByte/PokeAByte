using PokeAByte.Domain.Services.Mapper;

namespace PokeAByte.Utility.BuildMapperTree;

class Program
{
    static void Main(string[] args)
    {
        /*if(args.Length == 0)
            Console.WriteLine("Please provide the mapper directory or drag-and-drop the mapper directory onto the exe.");*/
        var dir = args.FirstOrDefault() ?? AppDomain.CurrentDomain.BaseDirectory;
        if (string.IsNullOrWhiteSpace(dir))
            throw new InvalidOperationException("Exe dir is empty or null.");
        Console.WriteLine($"Searching through directory {dir}.");
        var mappers = MapperTreeUtility.GenerateMapperDtoTree(dir);
        if (mappers.Count == 0)
        {
            Console.WriteLine("Failed to save file, no mappers found.");
            Console.ReadKey();
            return;
        }
        var saved = MapperTreeUtility.SaveChanges(dir, mappers);
        Console.WriteLine(saved is false ? "Failed to save file." : "Saved file.");
        Console.ReadKey();
    }
}

