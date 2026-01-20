using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ValidReadTests;

public class ReadBCDTest
{
    [Fact]
    public async Task ReadIntegersLittleEndian()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([
            0b0001_0011, 0b0011_0111
        ]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
            <property name="0" type="binaryCodedDecimal" length="2" address="0x00" />
            """,
                "0x00", "0x0F", "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(1337, clientnotifier.PropertyChanges.Last().Value);
    }
}