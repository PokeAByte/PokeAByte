
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PokeAByte.Domain;

public static class JsonFile
{
    /// <summary>
    /// Deserializes the file with the given path, or returns the fallback value if one of the following is true:
    /// - The file does not exist.
    /// - The file is empty
    /// - The deserializers threw an exception.
    /// - The deserialized JSON is null.
    /// </summary>
    /// <typeparam name="T"> The type of the deserialized object. </typeparam>
    /// <param name="filePath"> Path to the JSON file. </param>
    /// <param name="type"> The JsonTypeInfo to deserialize with. </param>
    /// <param name="fallback"> Fallback value in case deserialization fails. </param>
    /// <returns> The deserialized object or fallback value. </returns>
    public static T Deserialize<T>(string filePath, JsonTypeInfo<T> type, T fallback)
    {
        if (!File.Exists(filePath)) {
            return fallback;
        }
        
        var settingsJson = File.ReadAllText(filePath);
        if (string.IsNullOrEmpty(settingsJson))
        {
            return fallback;
        }
    
        try
        {
            return JsonSerializer.Deserialize(settingsJson, type) ?? fallback;
        }
        catch (Exception)
        {
            return fallback;
        }
    }
}