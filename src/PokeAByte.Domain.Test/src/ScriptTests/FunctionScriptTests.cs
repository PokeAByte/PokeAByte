using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace PokeAByte.Domain.Test.ScriptTests;

public class FunctionScriptTests
{
    [Fact]
    public async Task ReadFunction()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var driver = new TestDriver([0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            new TestClientNotifier(),
            MapperTestHelper.CreateMapper(
                """
                <property name="mapped" type="string" read-function="map_bytes" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x01",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/read_function.js"),
            currentFolder
        );


        await instance.Read();
        await instance.Read();

        Assert.Equal("False", instance.Mapper.get_property_value("test.mapped"));

        driver.SetData([1]);
        await instance.Read();

        Assert.Equal("True", instance.Mapper.get_property_value("test.mapped"));
    }

    [Fact]
    public async Task AfterReadValueExpression()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var driver = new TestDriver([32]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            new TestClientNotifier(),
            MapperTestHelper.CreateMapper(
                """
                <property name="modulo_2" type="int" length="1" address="0x00" after-read-value-expression="x % 2" />
                <property name="minus_1" type="int" length="1" address="0x00" after-read-value-expression="x - 1" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x01",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/read_function.js"),
            currentFolder
        );


        await instance.Read();
        await instance.Read();

        Assert.Equal(0d, instance.Mapper.get_property_value("test.modulo_2"));
        Assert.Equal(31d, instance.Mapper.get_property_value("test.minus_1"));

        driver.SetData([23]);
        await instance.Read();

        Assert.Equal(1d, instance.Mapper.get_property_value("test.modulo_2"));
        Assert.Equal(22d, instance.Mapper.get_property_value("test.minus_1"));
    }

    [Fact]
    public async Task AfterReadValueFunction()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var driver = new TestDriver([32]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            new TestClientNotifier(),
            MapperTestHelper.CreateMapper(
                """
                <property 
                    name="incremented" 
                    type="int" 
                    length="1" 
                    address="0x00" 
                    after-read-value-function="increment_property"
                />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x01",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/after_read_function.js"),
            currentFolder
        );

        await instance.Read();
        await instance.Read();

        Assert.Equal(33d, instance.Mapper.get_property_value("test.incremented"));

        driver.SetData([23]);
        await instance.Read();

        Assert.Equal(24d, instance.Mapper.get_property_value("test.incremented"));
    }

    [Fact]
    public async Task ContainerProcessor()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var driver = new TestDriver([1, 2, 3, 4]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            new TestClientNotifier(),
            MapperTestHelper.CreateMapper(
                """
                <property name="game" type="byteArray" length="4" address="0x00" />
                <property name="script" type="byteArray" length="4" memoryContainer="script" address="0x00" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x04",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/containerprocessor.js"),
            currentFolder
        );

        await instance.Read();
        await instance.Read();

        Assert.Equal(new byte[] { 1, 2, 3, 4 }, instance.Mapper.get_property_value("test.game"));
        // zeros from the scripts preprocessor:
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, instance.Mapper.get_property_value("test.script"));

        await instance.WriteValue(instance.Mapper.Properties["test.script"], "[5, 6, 7, 8]", false);

        await instance.Read();
        await instance.Read();

        Assert.True(driver.LastWriteMatches([5, 6, 7, 8]));
        Assert.Equal(new byte[] { 5, 6, 7, 8 }, instance.Mapper.get_property_value("test.script"));
        Assert.Equal(new byte[] { 5, 6, 7, 8 }, instance.Mapper.get_property_value("test.game"));

        var logger = MapperTestHelper.ScriptLogger;
        Assert.Contains(logger.GetSpyData(), x => x.LogLevel == LogLevel.Trace && x.Content == "trace");
        Assert.Contains(logger.GetSpyData(), x => x.LogLevel == LogLevel.Debug && x.Content == "debug");
        Assert.Contains(logger.GetSpyData(), x => x.LogLevel == LogLevel.Information && x.Content == "info");
        Assert.Contains(logger.GetSpyData(), x => x.LogLevel == LogLevel.Warning && x.Content == "warn");
        Assert.Contains(logger.GetSpyData(), x => x.LogLevel == LogLevel.Error && x.Content == "error");
    }

    [Fact]
    public async Task WriteFunctionCanBlock()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var driver = new TestDriver([1, 2]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            new TestClientNotifier(),
            MapperTestHelper.CreateMapper(
                """
                <property name="property" type="byteArray" length="2" address="0x00" write-function="write" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x04",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/writefunction.js"),
            currentFolder
        );

        await instance.Read();
        await instance.Read();

        Assert.Equal(new byte[] { 1, 2 }, instance.Mapper.get_property_value("test.property"));

        await instance.WriteBytes(instance.Mapper.get_property("test.script"), [3, 4], false);

        await instance.Read();
        Assert.Equal(new byte[] { 1, 2 }, instance.Mapper.get_property_value("test.property"));
    }

    [Fact]
    public async Task ContainerProcessorFreeze()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var driver = new TestDriver([1, 2, 3, 4]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            new TestClientNotifier(),
            MapperTestHelper.CreateMapper(
                """
                <property name="game" type="byteArray" length="4" address="0x00" />
                <property name="script" type="byteArray" length="4" memoryContainer="script" address="0x00" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x04",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/containerprocessor.js"),
            currentFolder
        );

        await instance.StartProcessing();

        Assert.Equal(new byte[] { 1, 2, 3, 4 }, instance.Mapper.get_property_value("test.game"));
        // zeros from the scripts preprocessor:
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, instance.Mapper.get_property_value("test.script"));

        await instance.FreezeProperty(instance.Mapper.get_property("test.script"), [5, 6, 7, 8 ]);
        instance.MemoryContainerManager.Fill("script", 0, [1,2,3,4]);

        await Task.Delay(10);
        
        // Assert.True(driver.LastWriteMatches([5,6,7,8]));
        Assert.Equal(new byte[] { 5, 6, 7, 8 }, instance.Mapper.get_property_value("test.script"));
        await Task.Delay(10);
        Assert.Equal(new byte[] { 5, 6, 7, 8 }, instance.Mapper.get_property_value("test.game"));
    }
}