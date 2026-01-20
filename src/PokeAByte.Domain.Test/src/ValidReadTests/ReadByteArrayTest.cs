using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ValidReadTests;

public class ReadByteArrayTest
{
    [Fact]
    public async Task ReadArray()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0x0A, 0x0B, 0x0C, 0x0D, 0x0F]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="array" type="byteArray" address="0x00" length="5" />""",
                memoryStart: "0x00",
                memoryEnd: "0x05",
                system: "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(driver.Bytes, clientnotifier.PropertyChanges[0].Value);

        driver.SetData([0x0F, 0x0D, 0x0C, 0x0B, 0x0A]);
        await instance.Read();

        byte[] expected = [0x0F, 0x0D, 0x0C, 0x0B, 0x0A];
        Assert.Equal(expected, clientnotifier.PropertyChanges[0].Value);
    }

    // todo: Test bits="0-3" length="4" to be [x, x, x, x]
}