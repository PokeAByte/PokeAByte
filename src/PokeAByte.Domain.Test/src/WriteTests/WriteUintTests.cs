using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.WriteTests;

public class WriteUintTests
{
    [Fact]
    public async Task WriteLittleEndian()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0, 0, 0, 0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="uint" length="4" address="0x00" />""",
                "0x00", "0x03", "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(0u, clientnotifier.PropertyChanges[0].Value);
        clientnotifier.ResetChanges();

        await instance.WriteValue(instance.Mapper.get_property("test.0"), "256", false);
        await instance.Read();
        Assert.Single(driver.Writes);
        Assert.Equal([0, 0, 1, 0], driver.Writes.Last().Bytes);
        Assert.Equal(256u, clientnotifier.PropertyChanges.Last().Value);
    }

    [Fact]
    public async Task WriteBigEndian()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0, 0, 0, 0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="uint" length="4" address="0x00" />""",
                "0x00", "0x03", "NDS"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(0u, clientnotifier.PropertyChanges[0].Value);
        clientnotifier.ResetChanges();

        await instance.WriteValue(instance.Mapper.get_property("test.0"), "256", false);
        await instance.Read();
        Assert.Single(driver.Writes);
        Assert.True(driver.LastWriteMatches([0, 1, 0, 0]));
        Assert.Equal(256u, clientnotifier.PropertyChanges.Last().Value);
    }

    [Fact]
    public async Task WriteBits()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="uint" length="1" address="0x00" bits="4-7" />""",
                "0x00", "0x03", "NDS"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(0u, clientnotifier.PropertyChanges[0].Value);
        clientnotifier.ResetChanges();

        await instance.WriteValue(instance.Mapper.get_property("test.0"), "15", false);
        await instance.Read();
        Assert.Single(driver.Writes);
        Assert.Equal([0b1111_0000], driver.Writes.Last().Bytes);
        Assert.Equal(15u, clientnotifier.PropertyChanges.Last().Value);
    }

    [Fact]
    public async Task WriteBitsFreeze()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="uint" length="1" address="0x00" bits="4" />""",
                "0x00", "0x03", "NDS"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        clientnotifier.ResetChanges();

        await instance.WriteValue(instance.Mapper.get_property("test.0"), 0b1, true);
        await instance.Read();
        Assert.Equal([0b0001_0001], driver.Writes.Last().Bytes);
        driver.Writes.Clear();

        driver.SetData([0xFF]);
        await instance.Read();
        Assert.NotEmpty(driver.Writes);
        Assert.Equal([0xFF], driver.Writes.Last().Bytes);
        driver.Writes.Clear();

        driver.SetData([0x00]);
        await instance.Read();
        Assert.NotEmpty(driver.Writes);
        Assert.Equal([0x10], driver.Writes.Last().Bytes);
        driver.Writes.Clear();
    }

    [Fact]
    public async Task WriteToFrozenUpdatesBytes()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="uint" length="1" address="0x00" bits="4" />""",
                "0x00", "0x03", "NDS"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        clientnotifier.ResetChanges();

        await instance.WriteBytes(instance.Mapper.get_property("test.0"), [1], true);
        await instance.Read();
        Assert.Equal([1], driver.Writes.Last().Bytes);

        await instance.WriteBytes(instance.Mapper.get_property("test.0"), [0xFF], null);
        await instance.Read();
        await instance.Read();
        Assert.NotEmpty(driver.Writes);
        Assert.Equal([0xFF], instance.Mapper.get_property("test.0").BytesFrozen);

    }
}