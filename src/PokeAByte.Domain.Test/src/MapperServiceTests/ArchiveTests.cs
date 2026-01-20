

namespace PokeAByte.Domain.Test.MapperServiceTests;

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PokeAByte.Domain.Models.Mappers;
using Voyager.UnitTestLogger;
using Xunit;

public class ArchiveTests : MapperTestBase
{
    [Fact]
    public async Task ListsArchivedMapper()
    {
        Directory.CreateDirectory(Path.Combine(MapperService.MapperArchivePath, "archive_test/test"));
        File.WriteAllText(
            Path.Combine(MapperService.MapperArchivePath, "archive_test/test/custom.xml"),
            """<mapper name="Awesome Custom Mapper" platform="gba" />"""
        );

        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());

        Assert.NotEmpty(service.ListArchived());
        var archivedMapper = service.ListArchived().First();
        Assert.Equal("archive_test", archivedMapper.Path);
        Assert.Equal(new MapperFile("custom.xml", "test/custom.xml", null), archivedMapper.Mapper);
    }

    [Fact]
    public async Task DeletesMapper()
    {
        Directory.CreateDirectory(Path.Combine(MapperService.MapperArchivePath, "archive_test/test"));
        File.WriteAllText(
            Path.Combine(MapperService.MapperArchivePath, "archive_test/test/custom.xml"),
            """<mapper name="Awesome Custom Mapper" platform="gba" />"""
        );

        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());

        Assert.NotEmpty(service.ListArchived());
        var archivedMapper = service.ListArchived().First();
        service.DeleteArchive(archivedMapper.Path);

        Assert.Empty(service.ListArchived());
        Assert.False(Directory.Exists(Path.Combine(MapperService.MapperArchivePath, "archive_test/test")));
    }


    [Fact]
    public async Task RestoresAndArchivesMapper()
    {
        Directory.CreateDirectory(Path.Combine(MapperService.MapperArchivePath, "archive_test/test"));
        File.WriteAllText(
            Path.Combine(MapperService.MapperArchivePath, "archive_test/test/custom.xml"),
            """<mapper name="Awesome Custom Mapper" platform="gba" />"""
        );

        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());

        Assert.Empty(service.ListInstalled());
        Assert.False(File.Exists("./Mappers/test/custom.xml"));
        Assert.NotEmpty(service.ListArchived());
        var archivedMapper = service.ListArchived().First();
        var result = service.Restore(archivedMapper.Path);

        Assert.True(result);
        Assert.Empty(service.ListArchived());
        Assert.False(Directory.Exists(Path.Combine(MapperService.MapperArchivePath, "archive_test/test")));
        Assert.NotEmpty(service.ListInstalled());
        Assert.True(File.Exists("./Mappers/test/custom.xml"));

        result = service.Archive(service.ListInstalled().Select(mapper => mapper.Path));

        Assert.True(result);
        Assert.Empty(service.ListInstalled());
        var newArchive = service.ListArchived().First();
        Assert.True(File.Exists($"./MapperArchives/{newArchive.Path}/test/custom.xml"));
        Assert.NotEmpty(service.ListArchived());
    }

    [Fact]
    public async Task CreatesBackups()
    {
        WriteMapper("test/custom.xml", """<mapper name="Awesome Custom Mapper" platform="gba" />""");

        var service = new MapperService(new SpyLog<MapperService>(), new TestDownloadService());

        Assert.Single(service.ListInstalled());
        Assert.Empty(service.ListArchived());

        var archivedMapper = service.ListInstalled().First();
        var result = await service.Backup([archivedMapper.Path]);
        Assert.True(result);
        Assert.Single(service.ListArchived());
        Assert.Single(service.ListInstalled());

        var backup = service.ListArchived().First();
        Assert.True(File.Exists("./Mappers/test/custom.xml"));
        Assert.True(File.Exists($"./MapperArchives/{backup.Path}/test/custom.xml"));
    }
}