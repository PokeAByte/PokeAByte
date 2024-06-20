using GameHook.Domain.Models.Properties;
using PokeAByte.Web.Models;

namespace Sandbox;

class Program
{
    static void Main(string[] args)
    {
        if(args.Length != 1)
            return;
        var data = File.ReadAllLines(args[0]);
        var props = data
            .Select(x => new PropertyModel
            {
                Path = x
            })
            .ToList();
        MapperPropertyTree tree = new();
        foreach (var prop in props)
        {
            tree.AddProperty(prop);
        }
        
    }
}