namespace PokeAByte.Domain.Test.AppSettingsServiceTests;

using System.IO;
using System.Threading.Tasks;
using PokeAByte.Domain.Models;
using Voyager.UnitTestLogger;
using Xunit;

public class AppSettingsServiceTests 
{
    [Fact]
    public async Task IntegrationTest()
    {
        if (File.Exists("./settings.json"))
        {
            File.Delete("./settings.json");
        }
        var service = new AppSettingsService(new SpyLog<AppSettingsService>());
        var settings = service.Get();

        Assert.Equal("127.0.0.1", settings.RETROARCH_LISTEN_IP_ADDRESS);
        Assert.Equal(55355, settings.RETROARCH_LISTEN_PORT);
        Assert.Equal(64, settings.RETROARCH_READ_PACKET_TIMEOUT_MS);
        Assert.Equal(5, settings.DELAY_MS_BETWEEN_READS);
        Assert.Equal(-1, settings.PROTOCOL_FRAMESKIP);
        
        service.Set(new AppSettings() { PROTOCOL_FRAMESKIP = 5});
        settings = service.Get();
        Assert.Equal(5, settings.PROTOCOL_FRAMESKIP);

        Assert.False(File.Exists("./settings.json"));

        service.Save();
        Assert.True(File.Exists("./settings.json"));
        Assert.Equal(
            """
            {
              "RETROARCH_LISTEN_IP_ADDRESS": "127.0.0.1",
              "RETROARCH_LISTEN_PORT": 55355,
              "RETROARCH_READ_PACKET_TIMEOUT_MS": 64,
              "DELAY_MS_BETWEEN_READS": 5,
              "PROTOCOL_FRAMESKIP": 5
            }
            """,
            File.ReadAllText("./settings.json")
        );
    }
}