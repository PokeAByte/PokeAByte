using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.TypeReadTests;

public class CalculateAddressTest
{
    [Fact]
    public async Task ResolvesVariableInt()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" address="{dynamic}" />""",
                memoryStart: "0x00",
                memoryEnd: "0x09",
                system: "GB"
            ),
            driver
        );

        instance.Variables.Add("dynamic", 0);
        await instance.Read();
        await instance.Read();
        Assert.Equal(1, instance.Mapper.get_property("test.0").Value);

        instance.Variables["dynamic"] = 1;
        instance.Variables.Add("reload_addresses", true);
        await instance.Read();
        Assert.Equal(2, instance.Mapper.get_property("test.0").Value);
    }

    [Fact]
    public async Task ResolvesVariableDouble()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" address="{dynamic}" />""",
                memoryStart: "0x00",
                memoryEnd: "0x09",
                system: "GB"
            ),
            driver
        );

        instance.Variables.Add("dynamic", 2D);
        await instance.Read();
        await instance.Read();
        Assert.Equal(3, instance.Mapper.get_property("test.0").Value);

        instance.Variables["dynamic"] = 1D;
        instance.Variables.Add("reload_addresses", true);
        await instance.Read();
        Assert.Equal(2, instance.Mapper.get_property("test.0").Value);
    }

    [Fact]
    public async Task CalculatesOffset()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" address="{dynamic}+2" />""",
                memoryStart: "0x00",
                memoryEnd: "0x09",
                system: "GB"
            ),
            driver
        );

        instance.Variables.Add("dynamic", 0);
        await instance.Read();
        await instance.Read();
        Assert.Equal(3, instance.Mapper.get_property("test.0").Value);

        instance.Variables["dynamic"] = 2;
        instance.Variables.Add("reload_addresses", true);
        await instance.Read();
        Assert.Equal(5, instance.Mapper.get_property("test.0").Value);
    }


    [Fact]
    public async Task UndefinedVariableYieldsNull()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" address="{dynamic}+2" />""",
                memoryStart: "0x00",
                memoryEnd: "0x09",
                system: "GB"
            ),
            driver
        );

        // instance.Variables.Add("dynamic", null);
        instance.Variables.Add("reload_addresses", true);
        await instance.Read();
        await instance.Read();
        Assert.Null(instance.Mapper.get_property("test.0").Value);

        Assert.DoesNotContain(MapperTestHelper.GetLogData(), x => x.LogLevel == Microsoft.Extensions.Logging.LogLevel.Error);
        Assert.Empty(clientnotifier.Errors);
    }

    [Fact]
    public async Task InvalidExpressionYieldsNull()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" address="{dynamic}()" />""",
                memoryStart: "0x00",
                memoryEnd: "0x09",
                system: "GB"
            ),
            driver
        );

        instance.Variables.Add("dynamic", "null");
        instance.Variables.Add("reload_addresses", true);
        await instance.Read();
        await instance.Read();

        Assert.Null(instance.Mapper.get_property("test.0").Value);
        Assert.Null(instance.Mapper.get_property("test.0").Address);

        await instance.Read();

        Assert.DoesNotContain(MapperTestHelper.GetLogData(), x => x.LogLevel == Microsoft.Extensions.Logging.LogLevel.Error);
        Assert.Empty(clientnotifier.Errors);
    }

    [Fact]
    public async Task NullParameterYieldsNull()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" address="{dynamic}()" />""",
                memoryStart: "0x00",
                memoryEnd: "0x09",
                system: "GB"
            ),
            driver
        );

        instance.Variables.Add("dynamic", null);
        instance.Variables.Add("reload_addresses", true);
        await instance.Read();
        await instance.Read();

        Assert.Null(instance.Mapper.get_property("test.0").Value);
        Assert.Null(instance.Mapper.get_property("test.0").Address);

        await instance.Read();

        Assert.DoesNotContain(MapperTestHelper.GetLogData(), x => x.LogLevel == Microsoft.Extensions.Logging.LogLevel.Error);
        Assert.Empty(clientnotifier.Errors);
    }

    [Fact]
    public async Task InvalidExpressionTypeYieldsNull()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" address="{dynamic}" />""",
                memoryStart: "0x00",
                memoryEnd: "0x09",
                system: "GB"
            ),
            driver
        );

        instance.Variables.Add("dynamic", "string");
        instance.Variables.Add("reload_addresses", true);
        await instance.Read();
        await instance.Read();

        Assert.Null(instance.Mapper.get_property("test.0").Value);
        Assert.Null(instance.Mapper.get_property("test.0").Address);

        Assert.DoesNotContain(MapperTestHelper.GetLogData(), x => x.LogLevel == Microsoft.Extensions.Logging.LogLevel.Error);
        Assert.Empty(clientnotifier.Errors);
    }
}
