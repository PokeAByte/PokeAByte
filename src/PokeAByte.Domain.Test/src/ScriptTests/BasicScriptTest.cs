using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ScriptTests;

public class BasicScriptTests
{
    [Fact]
    public async Task Loads()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="game-memory" type="bool" address="0x00" />
                <property name="byte-filled" type="bool" memoryContainer="script" address="0x00" />
                <property name="direct-set" type="bool" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x01",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/basic.js"),
            currentFolder
        );


        await instance.Read();
        await instance.Read();

        Assert.Contains(clientnotifier.PropertyChanges, x => x.Path == "test.game-memory" && x.Value is false);
        Assert.Contains(clientnotifier.PropertyChanges, x => x.Path == "test.byte-filled" && x.Value is true);
        Assert.Contains(clientnotifier.PropertyChanges, x => x.Path == "test.direct-set" && x.Value is true);
    }

    [Fact]
    public async Task CanAccessProperties()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([10, 20]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="game-memory-1" type="int" length="1" address="0x00" />
                <property name="game-memory-2" type="int" length="1" address="0x01" />
                <property name="script-set" type="int" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x02",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/addition.js"),
            currentFolder
        );

        await instance.Read();
        await instance.Read();
        var update = clientnotifier.PropertyChanges.FirstOrDefault(x => x.Path == "test.script-set");
        Assert.Equal(30d, update?.Value);
    }

    [Fact]
    public async Task CanBlockUpdates()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([10, 20]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="game-memory-1" type="int" length="1" address="0x00" />
                <property name="game-memory-2" type="int" length="1" address="0x01" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x02",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/block.js"),
            currentFolder
        );
        await instance.Read();
        await instance.Read();
        await instance.Read();
        await instance.Read();
        await instance.Read();
        Assert.Empty(clientnotifier.PropertyChanges);
    }

    [Fact]
    public async Task RunsPostProcessor()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([10, 20]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="pre" type="int" length="1" address="0x00" />
                <property name="post" type="int" length="1" address="0x01" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x02",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/postprocessor.js"),
            currentFolder
        );
        await instance.Read();
        Assert.NotEmpty(clientnotifier.PropertyChanges);

        var pre = clientnotifier.PropertyChanges.FirstOrDefault(x => x.Path == "test.pre");
        var post = clientnotifier.PropertyChanges.FirstOrDefault(x => x.Path == "test.post");

        // The preprocessor sets the property to 1, then the memory is processed, setting it to 10:
        Assert.Equal(10, pre.Value);

        // The postprocessor sets the value to the memory value + 1:
        Assert.Equal(21d, post.Value);
    }

    [Fact]
    public async Task CanReadBytesLittleEndian()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([
            16, 0,
            32, 0, 0, 0,
            64, 0, 0, 0, 0, 0, 0, 0,
        ]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="int16" type="int" />
                <property name="int32" type="int" />
                <property name="int64" type="int" />
                <property name="directInt16" type="int" />
                <property name="directInt32" type="int" />
                <property name="directInt64" type="int" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x15",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/le_bytereads.js"),
            currentFolder
        );
        await instance.Read();
        Assert.NotEmpty(clientnotifier.PropertyChanges);

        Assert.Equal(16d, instance.Mapper.get_property_value("test.int16"));
        Assert.Equal(32d, instance.Mapper.get_property_value("test.int32"));
        Assert.Equal(64d, instance.Mapper.get_property_value("test.int64"));

        Assert.Equal(16d, instance.Mapper.get_property_value("test.directInt16"));
        Assert.Equal(32d, instance.Mapper.get_property_value("test.directInt32"));
        Assert.Equal(64d, instance.Mapper.get_property_value("test.directInt64"));
    }

    [Fact]
    public async Task CanReadBytesBigEndian()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([
            0, 16,
            0, 0, 0, 32,
            0, 0, 0, 0, 0, 0, 0, 64,
        ]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="int16" type="int" />
                <property name="int32" type="int" />
                <property name="int64" type="int" />
                <property name="directInt16" type="int" />
                <property name="directInt32" type="int" />
                <property name="directInt64" type="int" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x15",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/be_bytereads.js"),
            currentFolder
        );
        await instance.Read();
        Assert.NotEmpty(clientnotifier.PropertyChanges);

        Assert.Equal(16d, instance.Mapper.get_property_value("test.int16"));
        Assert.Equal(32d, instance.Mapper.get_property_value("test.int32"));
        Assert.Equal(64d, instance.Mapper.get_property_value("test.int64"));
    }

    [Fact]
    public async Task CanWriteCustomMemoryNamespaces()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """
                <property name="int16" type="int" length="2" memoryContainer="script_memory" address="0x00" />
                <property name="int32" type="int" length="4" memoryContainer="script_memory" address="0x02" />
                """,
                memoryStart: "0x00",
                memoryEnd: "0x01",
                system: "GB"
            ),
            driver,
            Path.Combine(currentFolder, "scripts/dynamic_memory.js"),
            currentFolder
        );
        // await instance.Read();
        await instance.Read();
        Assert.NotEmpty(clientnotifier.PropertyChanges);

        Assert.Equal(16, instance.Mapper.get_property_value("test.int16"));
        Assert.Equal(32, instance.Mapper.get_property_value("test.int32"));
    }
}