using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ValidReadTests;

public class StaticValueTest
{
    [Fact]
    public async Task Read()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([2]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" value="42" address="0x00"  />""",
                memoryStart: "0x00",
                memoryEnd: "0x01",
                system: "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal("42", clientnotifier.PropertyChanges[0].Value);

        driver.SetData([1]);
        await instance.Read();

        Assert.Equal("42", clientnotifier.PropertyChanges[0].Value);
    }
}
