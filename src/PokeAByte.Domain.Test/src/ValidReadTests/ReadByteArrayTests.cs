using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ValidReadTests;

public class ReadByteArrayTests
{
    [Fact]
    public async Task ReadArray()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0, 1, 2, 3, 4, 5, 6, 7]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            """
        <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GB" version="1.0.0">
            <memory>
                <read start="0x00" end="0x08"/>
            </memory>
            <properties>
                <test>
                    <property name="array" type="byteArray" address="0x00" length="8" />
                </test>
            </properties>
        </mapper>
        """,
            driver
        );

        await instance.Read();
        await instance.Read();
        byte[] byteArray = [0, 1, 2, 3, 4, 5, 6, 7];
        Assert.Equal(byteArray, clientnotifier.PropertyChanges[0].Value);

        driver.SetData([7, 6, 5, 4, 3, 2, 1, 0]);
        await instance.Read();

        byteArray = [7, 6, 5, 4, 3, 2, 1, 0];
        Assert.Equal(byteArray, clientnotifier.PropertyChanges[0].Value);

        await instance.DisposeAsync();
    }
}