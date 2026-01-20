using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.WriteTests;

public class WriteBooleanTests
{
    [Fact]
    public async Task WriteWholeByte()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0, 0, 0, 0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="1" type="bool" address="0x00" />
                <property name="2" type="bool" address="0x01" />
                <property name="3" type="bool" address="0x02" />
                <property name="4" type="bool" address="0x03" />
                """,
                "0x00",
                "0x03",
                "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        await instance.WriteValue(instance.Mapper.get_property("test.1"), "true", false);
        await instance.WriteValue(instance.Mapper.get_property("test.2"), "true", false);
        await instance.WriteValue(instance.Mapper.get_property("test.3"), "true", false);
        await instance.WriteValue(instance.Mapper.get_property("test.4"), "true", false);
        Assert.Equal(4, driver.Writes.Count);
        Assert.Equal((uint)0, driver.Writes[0].Address);
        Assert.Equal([1], driver.Writes[0].Bytes);

        Assert.Equal((uint)1, driver.Writes[1].Address);
        Assert.Equal([1], driver.Writes[1].Bytes);

        Assert.Equal((uint)2, driver.Writes[2].Address);
        Assert.Equal([1], driver.Writes[2].Bytes);

        Assert.Equal((uint)3, driver.Writes[3].Address);
        Assert.Equal([1], driver.Writes[3].Bytes);
    }

    [Fact()]
    public async Task WriteBit()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0b0000_0000]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="bool" address="0x00" bits="4" />""",
                "0x00",
                "0x04",
                "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        clientnotifier.ResetChanges();
        await instance.WriteValue(instance.Mapper.get_property("test.0"), "true", false);
        // Wrote value:
        Assert.NotEmpty(driver.Writes);
        Assert.Equal((uint)0, driver.Writes.Last().Address);
        Assert.Equal([0b0001_0000], driver.Writes.Last().Bytes);
    }


    [Fact()]
    public async Task WriteBitFreeze()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0b0000_0000]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="bool" address="0x00" bits="4" />""",
                "0x00",
                "0x04",
                "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        clientnotifier.ResetChanges();
        await instance.WriteValue(instance.Mapper.get_property("test.0"), true, true);

        await instance.Read();
        // Wrote value:
        Assert.True(driver.LastWriteMatches([0b0001_0000]));
        // Marked as frozen:
        Assert.True(clientnotifier.PropertyChanges[0].IsFrozen);
        Assert.Equal(true, clientnotifier.PropertyChanges[0].Value);
    }

    [Fact()]
    public async Task WriteByteFreeze()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="bool" address="0x00"/>""",
                "0x00",
                "0x04",
                "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        clientnotifier.ResetChanges();
        await instance.WriteValue(instance.Mapper.get_property("test.0"), "true", true);
        await instance.Read();

        // Wrote value:
        Assert.True(driver.LastWriteMatches([1]));

        // Marked as frozen:
        Assert.True(clientnotifier.PropertyChanges[0].IsFrozen);
        Assert.Equal([1], clientnotifier.PropertyChanges[0].BytesFrozen);
        Assert.Equal(true, clientnotifier.PropertyChanges[0].Value);
        clientnotifier.ResetChanges();

        // Simulate change from the game:
        driver.SetData([0]);
        await instance.Read();
        Assert.Equal(3, driver.Writes.Count);
        Assert.Equal([1], driver.Writes.Last().Bytes);

    }
}