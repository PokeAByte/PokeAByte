

namespace PokeAByte.Domain.Test.PropertyTests;

using System.Threading.Tasks;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.PokeAByteProperties;
using Xunit;

public class ChangedFieldsTests
{
    [Fact]
    public async Task SetsCorrectFlags()
    {
        var property = new PokeAByteProperty(
            path: "test",
            EndianType: EndianTypes.LittleEndian,
            type: PropertyType.BinaryCodedDecimal,
            memoryContainer: null,
            address: null,
            length: 1,
            size: null,
            bits: null,
            reference: null,
            description: null,
            value: null
        );
        // Reset the changes from the initialization:
        property.FieldsChanged = FieldChanges.None;

        property.MemoryContainer = "script";
        Assert.True(property.FieldsChanged.HasFlag(FieldChanges.MemoryContainer));
        property.Address = 0x42;
        Assert.True(property.FieldsChanged.HasFlag(FieldChanges.Address));
        property.Length = 2;
        Assert.True(property.FieldsChanged.HasFlag(FieldChanges.Length));
        property.Size = 2;
        Assert.True(property.FieldsChanged.HasFlag(FieldChanges.Size));
        property.Bits = "1-4";
        Assert.True(property.FieldsChanged.HasFlag(FieldChanges.Bits));
        property.Reference = "lookup";
        Assert.True(property.FieldsChanged.HasFlag(FieldChanges.Reference));
        property.Description = "A test field";
        Assert.True(property.FieldsChanged.HasFlag(FieldChanges.Description));
        property.Value = 42;
        Assert.True(property.FieldsChanged.HasFlag(FieldChanges.Value));
        property.Bytes = [42];
        Assert.True(property.FieldsChanged.HasFlag(FieldChanges.Bytes));
        property.BytesFrozen = [42];
        Assert.True(property.FieldsChanged.HasFlag(FieldChanges.IsFrozen));

        // Assign all again, to check that the flags aren't improperly set again:
        property.FieldsChanged = FieldChanges.None;
        property.MemoryContainer = "script";
        property.Address = 0x42;
        property.Length = 2;
        property.Size = 2;
        property.Bits = "1-4";
        property.Reference = "lookup";
        property.Description = "A test field";
        property.Value = 42;
        property.Bytes = [42];
        property.BytesFrozen = [42];
        Assert.Equal(FieldChanges.None, property.FieldsChanged);

    }
}