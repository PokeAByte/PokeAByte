using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.PokeAByteProperties;

public class IntegerProperty : PokeAByteProperty
{
    public IntegerProperty(IPokeAByteInstance instance, PropertyAttributes variables) : base(instance, variables)
    {
    }

    protected override byte[] FromValue(string value)
    {
        if (Length == null) throw new Exception("Length is NULL.");
        var integerValue = int.Parse(value);
        var bytes = BitConverter.GetBytes(integerValue).Take(Length ?? 0).ToArray();
        return bytes.ReverseBytesIfLE(_endian);
    }

    protected override object? ToValue(byte[] data)
    {
        if (data.Length == 1) //  With one byte, we can just cast.
        {
            return (int)data[0];
        }
        // With less than 4 bytes, we have to pad the value before using the bitconverter:
        Span<byte> padded = stackalloc byte[4];
        data.AsSpan().CopyTo(padded);
        if (_endian == EndianTypes.LittleEndian)
        {
            padded.Slice(0, data.Length).Reverse();
        }
        return BitConverter.ToInt32(padded);
    }
}
