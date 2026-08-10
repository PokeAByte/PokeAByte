using System;
using System.Linq;
using System.Threading.Tasks;
using PokeAByte.Domain.Interfaces;
using Xunit;

namespace PokeAByte.Domain.Test.WriteTests;

public class WriteIntegerTests
{
    [Fact]
    public async Task WriteLittleEndianInteger()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0, 0, 0, 0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="4" address="0x00" />""",
                "0x00", "0x03", "GB"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(0, clientnotifier.PropertyChanges[0].Value);
        clientnotifier.ResetChanges();

        await instance.WriteValue(instance.Mapper.get_property("test.0"), "256", false);
        await instance.Read();
        Assert.Single(driver.Writes);
        Assert.Equal([0, 0, 1, 0], driver.Writes.Last().Bytes);
        Assert.Equal(256, clientnotifier.PropertyChanges.Last().Value);
    }

    [Fact]
    public async Task WriteBigEndian()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0, 0, 0, 0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="4" address="0x00" />""",
                "0x00", "0x03", "NDS"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(0, clientnotifier.PropertyChanges[0].Value);
        clientnotifier.ResetChanges();

        await instance.WriteValue(instance.Mapper.get_property("test.0"), "256", false);
        await instance.Read();
        Assert.Single(driver.Writes);
        Assert.True(driver.LastWriteMatches([0, 1, 0, 0]));
        Assert.Equal(256, clientnotifier.PropertyChanges.Last().Value);
    }

    [Fact]
    public async Task WriteBits()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" address="0x00" bits="4-7" />""",
                "0x00", "0x03", "NDS"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(0, clientnotifier.PropertyChanges[0].Value);
        clientnotifier.ResetChanges();

        await instance.WriteValue(instance.Mapper.get_property("test.0"), "15", false);
        await instance.Read();
        Assert.Single(driver.Writes);
        Assert.Equal([0b1111_0000], driver.Writes.Last().Bytes);
        Assert.Equal(15, clientnotifier.PropertyChanges.Last().Value);
    }

    [Fact]
    public async Task WriteMultipleBits()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="0" type="int" length="1" address="0x00" bits="0" />
                <property name="1" type="int" length="1" address="0x00" bits="1" />
                <property name="2" type="int" length="1" address="0x00" bits="2" />
                <property name="3" type="int" length="1" address="0x00" bits="3" />
                """,
                "0x00", "0x03", "NDS"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(0, clientnotifier.PropertyChanges[0].Value);
        clientnotifier.ResetChanges();

        var result = await instance.WriteMultiple(
            [
                new("test.0", "1"),
                new("test.1", "0"),
                new("test.2", "1"),
                new("test.3", "0"),
            ]
        );
        Assert.Equal(WriteMultipleResult.Success, result);
        await instance.Read();
        Assert.Single(driver.Writes);
        Assert.Equal([0b0000_0101], driver.Writes.Last().Bytes);
        Assert.Equal(4, clientnotifier.PropertyChanges.Count);
    }

    [Fact]
    public async Task WriteMultipleErrors()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="1_0" type="int" length="1" address="0x00" bits="0" />
                <property name="1_1" type="int" length="1" address="0x00" bits="1" />
                <property name="1_2" type="int" length="1" address="0x00" bits="2" />
                <property name="1_3" type="int" length="1" address="0x00" bits="3" />
                <property name="zero_length" type="int" length="0"  />
                <property name="no_address" type="int" length="2"  />
                <property name="wrong_length" type="int" length="2" address="0x0" bits="1" />
                <property name="wrong_index" type="int" length="1" address="0x0" bits="7" />
                <property name="wrong_address" type="int" length="1" address="0x01" bits="0" />
                """,
                "0x00", "0x03", "NDS"
            ),
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Equal(0, clientnotifier.PropertyChanges[0].Value);
        clientnotifier.ResetChanges();

        var result = await instance.WriteMultiple(
            [
                new("test.1_0", "1"),
                new("test.1_1", "0"),
                new("test.wrong", "1"),
            ]
        );
        Assert.Equal(WriteMultipleResult.InvalidPaths, result);


        result = await instance.WriteMultiple(
            [
                new("test.1_0", "1"),
                new("test.1_1", "0"),
                new("test.wrong_address", "1"),
            ]
        );
        Assert.Equal(WriteMultipleResult.InvalidProperty, result);

        result = await instance.WriteMultiple(
            [
                new("test.1_0", "1"),
                new("test.1_1", "0"),
                new("test.wrong_length", "1"),
            ]
        );
        Assert.Equal(WriteMultipleResult.InvalidProperty, result);

        result = await instance.WriteMultiple(
            [
                new("test.1_0", "1"),
                new("test.1_1", ""),
            ]
        );
        Assert.Equal(WriteMultipleResult.InvalidValue, result);

        result = await instance.WriteMultiple(
            [
                new("test.1_0", "1"),
                new("test.1_1", null),
            ]
        );
        Assert.Equal(WriteMultipleResult.InvalidValue, result);

        result = await instance.WriteMultiple(
            [
                new("test.no_address", "1"),
                new("test.1_0", "1"),
            ]
        );
        Assert.Equal(WriteMultipleResult.InvalidLengthOrAddress, result);

        result = await instance.WriteMultiple(
            [
                new("test.zero_length", "1"),
                new("test.1_0", "1"),
            ]
        );
        Assert.Equal(WriteMultipleResult.InvalidLengthOrAddress, result);

        Assert.Empty(driver.Writes);
        await instance.Read();
        Assert.Empty(clientnotifier.PropertyChanges);
    }

    [Fact]
    public async Task WriteBitsFreeze()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" address="0x00" bits="4" />""",
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
}