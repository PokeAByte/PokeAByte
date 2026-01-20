using System;
using System.Text.Json;
using System.Threading.Tasks;
using PokeAByte.Domain.Interfaces;
using Xunit;

namespace PokeAByte.Domain.Test.JsonTests;

public class JsonConverterTests
{
    [Fact]
    public async Task FieldChangesSerialize()
    {
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new FieldChangesJsonConverter());

        FieldChanges testValue = FieldChanges.Bytes | FieldChanges.Value;
        Assert.Equal("""["bytes","value"]""", JsonSerializer.Serialize(testValue, serializeOptions));

        testValue = FieldChanges.Bytes
            | FieldChanges.IsFrozen
            | FieldChanges.Bits
            | FieldChanges.Address
            | FieldChanges.Length
            | FieldChanges.Size
            | FieldChanges.Reference
            | FieldChanges.Description
            | FieldChanges.MemoryContainer;

        Assert.Equal(
            """["bytes","address","isFrozen","length","size","bits","reference","description","memoryContainer","frozen"]""",
            JsonSerializer.Serialize(testValue, serializeOptions)
        );
    }

    [Fact]
    public async Task ByteArraySerialize()
    {
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new ByteArrayJsonConverter());

        byte[] testValue = [42, 13, 37];

        // check the default behavior, just to illustrate the difference:
        Assert.Equal("\"Kg0l\"", JsonSerializer.Serialize(testValue));

        Assert.Equal("""[42,13,37]""", JsonSerializer.Serialize(testValue, serializeOptions));
    }

    [Fact]
    public async Task FieldChangesDeserialize()
    {
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new FieldChangesJsonConverter());

        Assert.Throws<NotImplementedException>(
            () => JsonSerializer.Deserialize<FieldChanges>("""["bytes","value"]""", serializeOptions)
        );
    }

    [Fact]
    public async Task ByteArrayDeserialize()
    {
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new ByteArrayJsonConverter());

        Assert.Equal([13, 37], JsonSerializer.Deserialize<byte[]>("""[13,37]""", serializeOptions));
    }
}