using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PokeAByte.Domain.Models;
using Xunit;

namespace PokeAByte.Domain.Test.MapperServiceTests;

public class MapperTestBase : IAsyncLifetime
{
    static SemaphoreSlim _semaphore = new(1, 1);

    public async Task InitializeAsync()
    {
        _semaphore.Wait();
        if (File.Exists("./github_api_settings.json"))
        {
            File.Delete("./github_api_settings.json");
        }
        if (Directory.Exists("./Mappers"))
        {
            Directory.Delete("./Mappers", recursive: true);
        }
        if (Directory.Exists("./MapperArchives"))
        {
            Directory.Delete("./MapperArchives", recursive: true);
        }
        Directory.CreateDirectory("./Mappers/test");
        BuildEnvironment.ConfigurationDirectory = "./";
    }

    public async Task DisposeAsync()
    {
        _semaphore.Release();
    }

    protected void WriteMapper(string path, string content)
    {
        Directory.CreateDirectory(Path.Combine(MapperService.MapperDirectory, Path.GetDirectoryName(path)));
        File.WriteAllText(
            Path.Combine(MapperService.MapperDirectory, path),
            content
        );
    }

    protected void WriteMapperTree(string content)
    {
        Directory.CreateDirectory(Path.Combine(MapperService.MapperDirectory));
        File.WriteAllText(
            Path.Combine(MapperService.MapperDirectory, "mapper_tree.json"),
            content
        );
    }
}