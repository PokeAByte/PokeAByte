using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Interfaces;

public class FieldChangesJsonConverter : JsonConverter<FieldChanges>
{
    public override FieldChanges Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("This converter is not intended for deserialization.");
    }

    public override void Write(Utf8JsonWriter writer, FieldChanges value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        if ((value & FieldChanges.Bytes) != 0)
            writer.WriteStringValue("bytes");
        if ((value & FieldChanges.Value) != 0)
            writer.WriteStringValue("value");
        if ((value & FieldChanges.Address) != 0)
            writer.WriteStringValue("address");
        if ((value & FieldChanges.IsFrozen) != 0)
            writer.WriteStringValue("isFrozen");
        if ((value & FieldChanges.Length) != 0)
            writer.WriteStringValue("length");
        if ((value & FieldChanges.Size) != 0)
            writer.WriteStringValue("size");
        if ((value & FieldChanges.Bits) != 0)
            writer.WriteStringValue("bits");
        if ((value & FieldChanges.Reference) != 0)
            writer.WriteStringValue("reference");
        if ((value & FieldChanges.Description) != 0)
            writer.WriteStringValue("description");
        if ((value & FieldChanges.MemoryContainer) != 0)
            writer.WriteStringValue("memoryContainer");
        if ((value & FieldChanges.IsFrozen) != 0)
            writer.WriteStringValue("frozen");
        writer.WriteEndArray();
    }
}