using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.MapperLoadTests;

public class PlatformTests
{

    [Fact]
    public async Task ThrowsOnBogusPlatform()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        var exception = Assert.Throws<MapperException>(() =>
        {
            MapperTestHelper.CreateTestInstance(
                clientnotifier,
                """
                <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="PC">
                    <properties>
                        <test>
                            <property name="0" type="boolean" length="1" address="0x00" />
                        </test>
                    </properties>
                </mapper>
                """,
                driver
            );
        });

        Assert.Equal(
            """Mapper specifies an unknown game platform PC.""",
            exception.Message
        );
    }

    [Fact]
    public async Task LoadsValidPlatforms()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        string[] validPlatforms = [
            "NES",
            "SNES",
            "GB",
            "GBC",
            "GBA",
            // "PSX", // throws. TODO: Fix.
            "NDS",
        ];
        foreach (var platform in validPlatforms)
        {
            var instance = MapperTestHelper.CreateTestInstance(
                clientnotifier,
                $"""
                <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="{platform}">
                    <properties>
                        <test>
                            <property name="0" type="bool" />
                        </test>
                    </properties>
                </mapper>
                """,
                driver
            );
            Assert.Single(instance.Mapper.Properties);
            Assert.NotEmpty(instance.Mapper.PlatformOptions.Ranges);
            await instance.DisposeAsync();
        }
    }
}
