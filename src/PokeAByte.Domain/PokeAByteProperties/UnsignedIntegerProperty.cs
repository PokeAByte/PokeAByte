using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.PokeAByteProperties;

public class UnsignedIntegerProperty : PokeAByteProperty, IPokeAByteProperty
{
    public UnsignedIntegerProperty(IPokeAByteInstance instance, PropertyAttributes variables) : base(instance, variables)
    {
    }

    protected override byte[] FromValue(string value)
    {
        if (Instance.PlatformOptions == null) throw new Exception("Instance.PlatformOptions is NULL.");
        if (Length == null) throw new Exception("Length is NULL.");

        int.TryParse(value, out var integerValue);
        byte[] bytes = BitConverter.GetBytes(integerValue)[..Length.Value];
        return bytes.ReverseBytesIfLE(Instance.PlatformOptions.EndianType);
    }

    protected override object? ToValue(byte[] data)
    {
        if (Instance == null) throw new Exception("Instance is NULL.");
        if (Instance.PlatformOptions == null) throw new Exception("Instance.PlatformOptions is NULL.");
        return GetUint(data, Instance.PlatformOptions);
    }

    public static uint GetUint(byte[] data, IPlatformOptions platformOptions) {
        // Shortcut: With one byte, we can just cast:
        if (data.Length == 1)
        {
            return data[0];
        }
        // With less than 4 bytes, we have to pad the value before using the bitconverter:
        if (data.Length >= 4) 
        {
            return BitConverter.ToUInt32(data.ReverseBytesIfLE(platformOptions.EndianType), 0);
        }
        // With less than 4 bytes, we have to pad the value before using the bitconverter:
        Span<byte> value = stackalloc byte[4];
        data.ReverseBytesIfLE(platformOptions.EndianType).AsSpan().CopyTo(value);
        return BitConverter.ToUInt32(value);
    }
}
