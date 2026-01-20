using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ValidReadTests;

public class ReadBooleanTests
{
    [Fact]
    public async Task ReadWholeByte()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="boolean" type="bool" address="0x00" />""",
                memoryStart: "0x00",
                memoryEnd: "0x01",
                system: "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(false, clientnotifier.PropertyChanges[0].Value);

        driver.SetData([1]);
        await instance.Read();
        Assert.Equal(true, clientnotifier.PropertyChanges[0].Value);

        driver.SetData([255]);
        await instance.Read();
        Assert.Equal(true, clientnotifier.PropertyChanges[0].Value);

        // Test non-empty string with unknown bytes:
        driver.SetData([128]);
        await instance.Read();
        Assert.Equal(true, clientnotifier.PropertyChanges[0].Value);
    }

    [Fact]
    public async Task ReadSelectBit()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="boolean" type="bool" address="0x00" bits="7" />""",
                memoryStart: "0x00",
                memoryEnd: "0x01",
                system: "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(false, clientnotifier.PropertyChanges[0].Value);

        driver.SetData([0b00000001]);
        await instance.Read();
        Assert.Equal(false, clientnotifier.PropertyChanges[0].Value);

        driver.SetData([0b10000001]);
        await instance.Read();
        Assert.Equal(true, clientnotifier.PropertyChanges[0].Value);

        // Test non-empty string with unknown bytes:
        driver.SetData([0b01111111]);
        await instance.Read();
        Assert.Equal(false, clientnotifier.PropertyChanges[0].Value);
    }
}