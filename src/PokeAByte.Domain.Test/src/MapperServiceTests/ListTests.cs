
namespace PokeAByte.Domain.Test.MapperServiceTests;

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Voyager.UnitTestLogger;
using Xunit;

public class ListTests : MapperTestBase
{
    [Fact]
    public async Task ListsUnmanagedMapper()
    {
        WriteMapper("test/custom.xml", """<mapper name="Awesome Custom Mapper" platform="gba" />""");

        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());
        Assert.NotEmpty(service.ListInstalled());
        Assert.Equal("test/custom.xml", service.ListInstalled().First().Path);
        Assert.Equal("custom.xml", service.ListInstalled().First().DisplayName);
        Assert.Null(service.ListInstalled().First().Version);
    }

    [Fact]
    public async Task ListEmptyArchive()
    {
        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());

        Assert.Empty(service.ListArchived());

        Directory.CreateDirectory(MapperService.MapperArchivePath);

        // Stays empty even with the mapper archive path existing:
        Assert.Empty(service.ListArchived());
    }
}