using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.WriteTests;

public class WriteBCDTest
{
    [Fact]
    public async Task WriteLittleEndian()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([
            0,0,0
        ]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="binaryCodedDecimal" length="2" address="0x00" />""",
                "0x00", "0x0F", "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(0, clientnotifier.PropertyChanges[0].Value);
        clientnotifier.ResetChanges();

        await instance.WriteValue(instance.Mapper.get_property("test.0"), 1337, false);
        await instance.Read();
        Assert.True(driver.LastWriteMatches([0b0001_0011, 0b0011_0111]));
        Assert.Equal(1337, clientnotifier.PropertyChanges.Last().Value);
    }
}