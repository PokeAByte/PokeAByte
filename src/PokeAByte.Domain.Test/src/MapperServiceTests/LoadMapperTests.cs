
namespace PokeAByte.Domain.Test.MapperServiceTests;

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Models;
using Voyager.UnitTestLogger;
using Xunit;

public class LoadMapperTests : MapperTestBase
{
    [Fact]
    public async Task LogsRemovedMappers()
    {
        WriteMapperTree("""[{"display_name": "Test mapper", "path": "test/xml_only.xml", "version": "0.9.99" }]""");

        var logger = new SpyLog<MapperService>();
        var service = new MapperService(logger, new TestDownloadService());
        Assert.Contains(logger.GetSpyData(), x => x.LogLevel == LogLevel.Information);
        Assert.Equal(
            "Some mappers could no longer be found in the mapper folder.",
            logger.GetSpyData().FirstOrDefault(x => x.LogLevel == LogLevel.Information).Content
        );
    }

    [Fact]
    public async Task ThrowsOnMissingMapper()
    {
        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await service.LoadContentAsync("test/badmapper.xml"));
    }

    [Fact]
    public async Task LoadsExistingMapperContent()
    {
        WriteMapper("test/gameboy.xml", """<mapper />""");
        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());
        var content = await service.LoadContentAsync("test/gameboy.xml");
        Assert.NotNull(content);
        Assert.Equal("<mapper />", content.Xml);
        Assert.Null(content.ScriptPath);
    }

    [Fact]
    public async Task LoadsExistingMapperContentWithScript()
    {
        WriteMapper("test/gameboy_script.xml", """<mapper />""");
        File.WriteAllText("./Mappers/test/gameboy_script.js", """export function preprocessor() {}""");

        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());
        var content = await service.LoadContentAsync("test/gameboy_script.xml");
        Assert.NotNull(content);
        Assert.Equal("<mapper />", content.Xml);
        Assert.Equal("./Mappers/test/gameboy_script.js", content.ScriptPath);
    }
}
