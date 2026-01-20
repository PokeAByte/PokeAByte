using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ModelTests;

public class DownloadSettingsTests
{
    [Fact]
    public async Task DefaultValues()
    {
        var testObject = new DownloadSettings();

        Assert.Equal("", testObject.Token);
        Assert.Equal("PokeAByte", testObject.Owner);
        Assert.Equal("mappers", testObject.Repo);
        Assert.Equal("", testObject.Directory);
        Assert.Null(testObject.GetFormattedToken());
        Assert.Equal("https://github.com/PokeAByte/mappers/", testObject.GetGithubUrl());
    }
}