using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.MapperLoadTests;

public class MapperSyntaxVersionTests
{
    [Fact]
    public async Task ThrowsIfIncompatible()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        var exception = Assert.Throws<MapperException>(() =>
        {
            return MapperTestHelper.CreateTestInstance(
                clientnotifier,
                """
                <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GBA" syntax="9999">
                    <memory>
                        <read start="0x00" end="0x01"/>
                    </memory>
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
            "The mapper test.xml is too new. Please update Poke-A-Byte.",
            exception.Message
        );
    }

    [Fact]
    public async Task AcceptsMissingSyntax()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);

        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            """
            <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GBA">
                <memory>
                    <read start="0x00" end="0x01"/>
                </memory>
                <properties>
                    <test>
                        <property name="0" type="bool" length="1" address="0x00" />
                    </test>
                </properties>
            </mapper>
            """,
            driver
        );
    }

    [Fact]
    public async Task AcceptsLatestSynax()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);

        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            """
            <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GBA" syntax="4">
                <memory>
                    <read start="0x00" end="0x01"/>
                </memory>
                <properties>
                    <test>
                        <property name="0" type="bool" length="1" address="0x00" />
                    </test>
                </properties>
            </mapper>
            """,
            driver
        );
    }
}