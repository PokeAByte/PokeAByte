using System.Text.Json.Serialization;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Models.Properties;

public class PropertyModel
{
    public string Path { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? MemoryContainer { get; set; } = string.Empty;
    public string? OriginalAddressString { get; set; }
    public uint? Address { get; set; }

    public int? Length { get; set; }

    public int? Size { get; set; }

    public string? Reference { get; set; }

    public string? Bits { get; set; }

    public string? Description { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public object? Value { get; set; }

    public int[]? Bytes { get; set; } = [];

    public bool? IsFrozen { get; set; }

    public bool IsReadOnly { get; set; }
    public required IPokeAByteProperty BaseProperty { get; set; }
    public HashSet<string> FieldsChanged { get; set; } = [];
}

public static class PropertyModelExtensions
{
    public static void UpdateValues(this PropertyModel? current, PropertyModel? updated)
    {
        if (current is null || updated is null) return;
        current.Path = updated.Path;
        current.Type = updated.Type;
        current.MemoryContainer = updated.MemoryContainer;
        current.Address = updated.Address;
        current.Length = updated.Length;
        current.Size = updated.Size;
        current.Reference = updated.Reference;
        current.Bits = updated.Bits;
        current.Description = updated.Description;
        current.Value = updated.Value;
        current.Bytes = updated.Bytes;
        current.IsFrozen = updated.IsFrozen;
        current.IsReadOnly = updated.IsReadOnly;
    }
    public static void UpdatePropertyModel(this PropertyModel original, IPokeAByteProperty updated)
    {
        if (updated.Path != original.Path)
            return;

        if (updated.Value != original.Value)
        {
            original.Value = updated.Value;
        }
        if (updated.FieldsChanged.Contains("bytes"))
        {
            original.Bytes = updated.Bytes?.ToIntegerArray();
        }

        if (updated.IsFrozen != original.IsFrozen)
            original.IsFrozen = updated.IsFrozen;
        if (updated.IsReadOnly != original.IsReadOnly)
            original.IsFrozen = updated.IsReadOnly;

        original.FieldsChanged = new HashSet<string>(updated.FieldsChanged);
    }

    public static string ValueAsString(this PropertyModel model)
    {
        if (model is { Type: "bitArray", Value: bool[] bArray })
        {
            return bArray.Aggregate("", (current, b) => current + (b ? "1" : "0"));
        }
        return model.Value?.ToString() ?? "";
    }
}