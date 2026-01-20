
namespace PokeAByte.Domain.Test.MapperServiceTests;

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Voyager.UnitTestLogger;
using Microsoft.Extensions.Logging;
using Xunit;

public class DownloadTests : MapperTestBase
{

    [Fact]
    public async Task SavesDownloadedMappers()
    {
        Assert.False(File.Exists("./Mappers/remote_mappers.json"));
        Assert.False(File.Exists("./Mappers/test/xml_only.xml"));

        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());
        Assert.Empty(service.ListInstalled());
        Assert.True(File.Exists("./Mappers/remote_mappers.json"));

        var downloadSuccess = await service.DownloadAsync([service.ListRemote().First().Path]);

        Assert.True(downloadSuccess);
        Assert.NotEmpty(service.ListInstalled());
        Assert.True(File.Exists("./Mappers/test/xml_only.xml"));
        Assert.Equal(
            """<mapper name="xml_only" />""",
            File.ReadAllText("./Mappers/test/xml_only.xml")
        );
    }

    [Fact]
    public async Task SavesDownloadedMappersWithScript()
    {
        Assert.False(File.Exists("./Mappers/remote_mappers.json"));
        Assert.False(File.Exists("./Mappers/test/xml_script.xml"));

        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());
        Assert.Empty(service.ListInstalled());
        Assert.True(File.Exists("./Mappers/remote_mappers.json"));

        var downloadSuccess = await service.DownloadAsync([service.ListRemote().Last().Path]);

        Assert.True(downloadSuccess);
        Assert.NotEmpty(service.ListInstalled());

        Assert.True(File.Exists("./Mappers/test/xml_script.xml"));
        Assert.Equal(
            """<mapper name="xml_script" />""",
            File.ReadAllText("./Mappers/test/xml_script.xml")
        );

        Assert.True(File.Exists("./Mappers/test/xml_script.js"));
        Assert.Equal(
            """export function preprocessor() { return false; }""",
            File.ReadAllText("./Mappers/test/xml_script.js")
        );
    }

    [Fact]
    public async Task LogsWarningIfMapperUnknown()
    {
        var logger = new SpyLog<MapperService>();
        var service = new MapperService(logger, new TestDownloadService());
        Assert.Empty(service.ListInstalled());
        Assert.True(File.Exists("./Mappers/remote_mappers.json"));

        var downloadSuccess = await service.DownloadAsync(["test/invalid_mapper.xml"]);

        Assert.Empty(service.ListInstalled());
        Assert.True(downloadSuccess);
        Assert.Contains(logger.GetSpyData(), x => x.LogLevel == LogLevel.Warning);
        Assert.Equal(
            "Skipped update/download for test/invalid_mapper.xml because it's not in the list of remote mappers.",
            logger.GetSpyData().FirstOrDefault(x => x.LogLevel == LogLevel.Warning).Content
        );
    }

    [Fact]
    public async Task UpdatesExistingMapper()
    {
        // Mock an installed mapper:
        WriteMapper("test/xml_only.xml", """<mapper name="xml_only" platform="nonsense" />""");
        WriteMapperTree("""[{"display_name": "Test mapper", "path": "test/xml_only.xml", "version": "0.9.99" }]""");

        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());

        // Check that the mock mapper is read correctly:
        Assert.NotEmpty(service.ListInstalled());
        Assert.Equal("0.9.99", service.ListInstalled().First().Version);
        Assert.True(File.Exists("./Mappers/remote_mappers.json"));

        var downloadSuccess = await service.DownloadAsync([service.ListRemote().First().Path]);

        Assert.True(downloadSuccess);
        Assert.Equal("1.0.0", service.ListInstalled().First().Version);
        File.WriteAllText(
            "./Mappers/test/xml_only.xml",
            """<mapper name="xml_only" />"""
        );
    }
}
