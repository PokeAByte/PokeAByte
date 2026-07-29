using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Interfaces;

[JsonSerializable(typeof(byte))]
public partial class ConverterContext : JsonSerializerContext;

public class ByteArrayJsonConverter : JsonConverter<byte[]>
{
    public override byte[] Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }
        reader.Read();

        var elements = new List<byte>();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            elements.Add(JsonSerializer.Deserialize(ref reader, ConverterContext.Default.Byte)!);

            reader.Read();
        }

        return elements.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (byte item in value)
        {
            JsonSerializer.Serialize(writer, item, ConverterContext.Default.Byte);
        }

        writer.WriteEndArray();
    }
}
