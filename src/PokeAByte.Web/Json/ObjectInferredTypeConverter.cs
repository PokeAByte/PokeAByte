using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokeAByte.Web.Json;

public class ObjectToInferredTypesConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number when reader.TryGetInt64(out long longValue) => longValue,
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.String => reader.GetString()!,
            _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
        };
    }

    public override void Write(Utf8JsonWriter writer, object objectToWrite, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
