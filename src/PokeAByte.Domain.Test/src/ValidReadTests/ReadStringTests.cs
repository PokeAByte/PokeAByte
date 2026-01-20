using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ValidReadTests;

public class ReadStringTests
{
    [Fact]
    public async Task ReadSingleCharacterString()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
        ]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="string" type="string" length="8" address="0x00" />""",
                memoryStart: "0x00",
                memoryEnd: "0x0F",
                system: "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal("", clientnotifier.PropertyChanges[0].Value);

        // Test string that does not terminate properly:
        driver.SetData([1, 2, 3, 4, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1]);
        await instance.Read();
        Assert.Equal("abcdaaaa", clientnotifier.PropertyChanges[0].Value);

        // Test non-empty string that  terminates before full length:
        driver.SetData([1, 2, 3, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1]);
        await instance.Read();
        Assert.Equal("abc", clientnotifier.PropertyChanges[0].Value);

        // Test non-empty string with unknown bytes:
        driver.SetData([1, 2, 3, 255, 255, 255, 255, 0, 1, 1, 1, 1, 1, 1, 1, 1]);
        await instance.Read();
        Assert.Equal("abc", clientnotifier.PropertyChanges[0].Value);
    }

    [Fact]
    public async Task ReadWideCharacterStringLittleEndian()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
        ]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            """
            <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GB" version="1.0.0">
                <memory>
                    <read start="0x00" end="0x0F"/>
                </memory>
                <properties>
                    <test>
                        <property name="string" type="string" size="2" length="16" address="0x00" />
                    </test>
                </properties>
                <references>
                    <defaultCharacterMap>
                        <entry key="0x00" />
                        <entry key="0x01" value="a" />
                        <entry key="0x02" value="b" />
                        <entry key="0x03" value="c" />
                        <entry key="0x04" value="d" />
                        <entry key="0x05" value="e" />
                        <entry key="0x06" value="f" />
                        <entry key="0x07" value="g" />
                        <entry key="0x08" value="h" />
                    </defaultCharacterMap>
                </references>
            </mapper>
            """,
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal("", clientnotifier.PropertyChanges[0].Value);

        // Test string that does not terminate properly:
        driver.SetData([0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1]);
        await instance.Read();
        Assert.Equal("aaaaaaaa", clientnotifier.PropertyChanges[0].Value);

        // Test non-empty string that terminates before full length:
        driver.SetData([0, 1, 0, 3, 0, 8, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1]);
        await instance.Read();
        Assert.Equal("ach", clientnotifier.PropertyChanges[0].Value);

        // Test non-empty string with unknown bytes:
        driver.SetData([0, 1, 0, 3, 0, 8, 0, 10, 0, 10, 0, 10, 0, 10, 0, 10]);
        await instance.Read();
        Assert.Equal("ach", clientnotifier.PropertyChanges[0].Value);
    }

    [Fact]
    public async Task ReadWideCharacterStringBigEndian()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
        ]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            """
            <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="NDS" version="1.0.0">
                <memory>
                    <read start="0x00" end="0x0F"/>
                </memory>
                <properties>
                    <test>
                        <property name="string" type="string" size="2" length="16" address="0x00" />
                    </test>
                </properties>
                <references>
                    <defaultCharacterMap>
                        <entry key="0x00" />
                        <entry key="0x01" value="a" />
                        <entry key="0x02" value="b" />
                        <entry key="0x03" value="c" />
                        <entry key="0x04" value="d" />
                        <entry key="0x05" value="e" />
                        <entry key="0x06" value="f" />
                        <entry key="0x07" value="g" />
                        <entry key="0x08" value="h" />
                    </defaultCharacterMap>
                </references>
            </mapper>
            """,
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal("", clientnotifier.PropertyChanges[0].Value);

        // Test string that does not terminate properly:
        driver.SetData([1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0]);
        await instance.Read();
        Assert.Equal("aaaaaaaa", clientnotifier.PropertyChanges[0].Value);

        // Test non-empty string that terminates before full length:
        driver.SetData([1, 0, 3, 0, 8, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0]);
        await instance.Read();
        Assert.Equal("ach", clientnotifier.PropertyChanges[0].Value);

        // Test non-empty string with unknown bytes:
        driver.SetData([1, 0, 3, 0, 8, 0, 10, 0, 10, 0, 10, 0, 10, 0, 10, 0]);
        await instance.Read();
        Assert.Equal("ach", clientnotifier.PropertyChanges[0].Value);
    }
}
