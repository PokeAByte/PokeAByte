using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ValidReadTests;

public class ReadBitArrayTest
{
    [Fact]
    public async Task ReadAllBits()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0xFF]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="array" type="bitArray" address="0x00" />""",
                memoryStart: "0x00",
                memoryEnd: "0x01",
                system: "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        bool[] expected = [true, true, true, true, true, true, true, true];
        Assert.Equal(expected, clientnotifier.PropertyChanges[0].Value);

        driver.SetData([0]);
        await instance.Read();

        expected = [false, false, false, false, false, false, false, false];
        Assert.Equal(expected, clientnotifier.PropertyChanges[0].Value);
    }

    // todo: Test bits="0-3" length="4" to be [x, x, x, x]
}