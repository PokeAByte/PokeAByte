using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PokeAByte.Domain;

internal static class JsonFile
{
    internal static void Write<T>(T value, string path, JsonTypeInfo<T> typeInfo)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(value, typeInfo));
    }

    /// <summary>
    /// Deserializes the file with the given path, or returns the fallback value if one of the following is true:
    /// - The file does not exist.
    /// - The file is empty
    /// - The deserializers threw an exception.
    /// - The deserialized JSON is null.
    /// </summary>
    /// <typeparam name="T"> The type of the deserialized object. </typeparam>
    /// <param name="path"> Path to the JSON file. </param>
    /// <param name="typeInfo"> The JsonTypeInfo to deserialize with. </param>
    /// <returns> The deserialized object or fallback value. </returns>
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