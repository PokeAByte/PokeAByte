using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.WriteTests;

public class WriteByteArrayTest
{
    [Fact]
    public async Task WriteArray()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0, 0, 0, 0, 0]);
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
        Assert.Equal(new byte[5] { 0, 0, 0, 0, 0 }, clientnotifier.PropertyChanges[0].Value);

        await instance.WriteValue(instance.Mapper.get_property("test.array"), "[1, 2, 3, 4, 5]", null);
        await instance.Read();
        Assert.Single(driver.Writes);
        Assert.Equal(new byte[5] { 1, 2, 3, 4, 5 }, driver.Writes.Last().Bytes);
        Assert.Equal(new byte[5] { 1, 2, 3, 4, 5 }, clientnotifier.PropertyChanges[0].Value);
    }

    // todo: Test bits="0-3" length="4" to be [x, x, x, x]
}