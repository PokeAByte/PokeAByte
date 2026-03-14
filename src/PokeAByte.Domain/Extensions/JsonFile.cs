using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PokeAByte.Domain;

internal static class JsonFile
{
    internal static void Write<T>(T value, string path, JsonTypeInfo<T> typeInfo)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(value, typeInfo));
    }

    internal static T? Read<T>(string path, JsonTypeInfo<T> typeInfo) where T : class
    {
        if (!File.Exists(path))
        {
            return null;
        }
        string jsonContent = File.ReadAllText(path);
        return string.IsNullOrEmpty(jsonContent)
            ? null
            : JsonSerializer.Deserialize(jsonContent, typeInfo);
    }
}