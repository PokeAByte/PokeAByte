using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.WriteTests;

public class WriteReferenceValue
{
    [Fact]
    public async Task WriteReferenceByValue()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1]);
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
        Assert.Equal("one", clientnotifier.PropertyChanges[0].Value);

        await instance.WriteValue(instance.Mapper.get_property("test.0"), "two", false);
        await instance.Read();

        Assert.NotEmpty(driver.Writes);
        Assert.Equal([2], driver.Writes.Last().Bytes);
        Assert.Equal("two", clientnotifier.PropertyChanges[0].Value);
    }

    [Fact]
    public async Task WriteReferenceByKey()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1]);
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
        Assert.Equal("one", clientnotifier.PropertyChanges[0].Value);

        await instance.WriteValue(instance.Mapper.get_property("test.0"), 2L, false);
        await instance.Read();

        Assert.NotEmpty(driver.Writes);
        Assert.Equal([2], driver.Writes.Last().Bytes);
        Assert.Equal("two", clientnotifier.PropertyChanges[0].Value);
    }

    [Fact]
    public async Task FailedReferenceWrite()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1]);
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

        Exception ex = await Assert.ThrowsAsync<Exception>(
            async () => await instance.WriteValue(instance.Mapper.get_property("test.0"), "10", false)
        );
        Assert.Equal("Missing dictionary value for '10', value was not found in reference list digits.", ex.Message);

        ex = await Assert.ThrowsAsync<Exception>(
            async () => await instance.WriteValue(instance.Mapper.get_property("test.0"), "ten", false)
        );
        Assert.Equal("Missing dictionary value for 'ten', value was not found in reference list digits.", ex.Message);

        Assert.Empty(driver.Writes);
    }
}