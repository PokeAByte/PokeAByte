using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.TypeReadTests;

public class InvalidLengthTest
{
    [Fact]
    public async Task ReadTooManyBytes()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            """
            <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GB" version="1.0.0">
                <memory>
                    <read start="0x00" end="0x01"/>
                </memory>
                <properties>
                    <test>
                        <property name="array" type="byteArray" address="0x00" length="4" />
                    </test>
                </properties>
            </mapper>
            """,
            driver
        );

        await instance.Read();
        await instance.Read();
        await instance.DisposeAsync();
        Assert.NotEmpty(clientnotifier.Errors);
        Assert.Equal("Error", clientnotifier.Errors.Last().Title);
        Assert.Equal(
            "Unable to retrieve bytes for property 'test.array': Cannot retrieve bytes starting at 00 because"
            + " getting 4 bytes would overflow the memory container.",
            clientnotifier.Errors.Last().Detail
        );
    }
}