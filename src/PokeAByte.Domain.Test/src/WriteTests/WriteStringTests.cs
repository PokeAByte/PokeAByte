using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.WriteTests;

public class WriteStringTests
{
    [Fact]
    public async Task WriteSingleByteChars()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0, 0, 0, 0, 0, 0, 0, 0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
            """
        <property name="1" type="string" length="8" address="0x00" />    
        """, "0x00", "0x08", "GB"),
            driver
        );

        await instance.StartProcessing();
        await instance.WriteValue(instance.Mapper.get_property("test.1"), "hello", false);
        Assert.Single(driver.Writes);
        Assert.Equal((uint)0, driver.Writes[0].Address);
        Assert.Equal([8, 5, 12, 12, 15, 0, 0, 0], driver.Writes[0].Bytes);
    }
}