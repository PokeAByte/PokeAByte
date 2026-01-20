using System.Threading.Tasks;
using PokeAByte.Domain.Interfaces;
using Xunit;

namespace PokeAByte.Domain.Test.TypeReadTests;

public class ReadIntegerTests
{
    [Fact]
    public async Task ReadIntegersLittleEndian()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1,
        ]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="integer-1" type="int" length="1" address="0x00" />
                <property name="integer-2" type="int" length="2" address="0x04" />
                <property name="integer-3" type="int" length="3" address="0x08" />
                <property name="integer-4" type="int" length="4" address="0x0C" />
                """,
                "0x00", "0x0F", "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(1, clientnotifier.PropertyChanges[0].Value);
        Assert.Equal(1, clientnotifier.PropertyChanges[1].Value);
        Assert.Equal(1, clientnotifier.PropertyChanges[2].Value);
        Assert.Equal(1, clientnotifier.PropertyChanges[3].Value);

        driver.SetData([
            255, 0, 0, 0,
            1, 0, 0, 0,
            1, 0, 0, 0,
            1, 0, 0, 0,
        ]);
        await instance.Read();

        Assert.Equal(255, clientnotifier.PropertyChanges[0].Value);
        Assert.Equal(256, clientnotifier.PropertyChanges[1].Value);
        Assert.Equal(65536, clientnotifier.PropertyChanges[2].Value);
        Assert.Equal(16777216, clientnotifier.PropertyChanges[3].Value);
    }

    [Fact]
    public async Task ReadIntegersBigEndian()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([
            1, 0, 0, 0,
            2, 0, 0, 0,
            3, 0, 0, 0,
            4, 0, 0, 0,
        ]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            new MapperContent(
                "test",
                MapperTestHelper.CreateMapper(
                    """
                    <property name="integer-1" type="int" length="1" address="0x00" />
                    <property name="integer-2" type="int" length="2" address="0x04" />
                    <property name="integer-3" type="int" length="3" address="0x08" />
                    <property name="integer-4" type="int" length="4" address="0x0C" />
                    """,
                    "0x00", "0x0F", "NDS"
                ),
                null, null
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.NotNull(instance.Mapper);
        Assert.Equal(4, instance.Mapper.Properties.Values.Count);
        Assert.Equal(4, clientnotifier.PropertyChanges.Count);
        Assert.Equal(1, clientnotifier.PropertyChanges[0].Value);
        Assert.Equal(2, clientnotifier.PropertyChanges[1].Value);
        Assert.Equal(3, clientnotifier.PropertyChanges[2].Value);
        Assert.Equal(4, clientnotifier.PropertyChanges[3].Value);

        driver.SetData([
            255, 0, 0, 0,
            255, 255, 0, 0,
            255, 255, 255, 0,
            255, 255, 255, 255,
        ]);
        await instance.Read();

        Assert.Equal((2 << 7) - 1, clientnotifier.PropertyChanges[0].Value);
        Assert.Equal((2 << 15) - 1, clientnotifier.PropertyChanges[1].Value);
        Assert.Equal((2 << 23) - 1, clientnotifier.PropertyChanges[2].Value);
        Assert.Equal((2 << 31) - 1, clientnotifier.PropertyChanges[3].Value);
    }

    [Fact]
    public async Task ReadSelectBitsLower()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="integer-1" type="int" length="1" address="0x00" bits="0-3" />
                """,
                "0x00", "0x03", "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(1, clientnotifier.PropertyChanges[0].Value);


        driver.SetData([0b1111_1111]);
        await instance.Read();
        Assert.Equal(15, clientnotifier.PropertyChanges[0].Value);

        driver.SetData([0b1111_0010]);
        await instance.Read();
        Assert.Equal(2, clientnotifier.PropertyChanges[0].Value);
    }

    [Fact]
    public async Task ReadSelectBitsUpper()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="integer-1" type="int" length="1" address="0x00" bits="4-7" />
                """,
                "0x00", "0x03", "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(0, clientnotifier.PropertyChanges[0].Value);

        driver.SetData([0b1111_0000]);
        await instance.Read();
        Assert.Equal(15, clientnotifier.PropertyChanges[0].Value);

        driver.SetData([0b0010_0000]);
        await instance.Read();
        Assert.Equal(2, clientnotifier.PropertyChanges[0].Value);
    }
}