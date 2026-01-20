using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ValidReadTests;

public class ReadReferenceValue
{
    [Fact]
    public async Task ReadReference()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([2]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" address="0x00" reference="digits" />""",
                memoryStart: "0x00",
                memoryEnd: "0x01",
                system: "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal("two", clientnotifier.PropertyChanges[0].Value);

        driver.SetData([1]);
        await instance.Read();

        Assert.Equal("one", clientnotifier.PropertyChanges[0].Value);
    }
}