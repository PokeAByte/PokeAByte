using System.Collections;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.PokeAByteProperties;

public class BitFieldProperty : PokeAByteProperty, IPokeAByteProperty
{
    public BitFieldProperty(IPokeAByteInstance instance, PropertyAttributes variables) : base(instance, variables)
    {
    }

    protected override byte[] FromValue(string value)
    {
        throw new NotImplementedException();
    }

    protected override object? ToValue(byte[] data)
    {
        var bitArray = new BitArray(data);

        var boolArray = new bool[bitArray.Length];
        bitArray.CopyTo(boolArray, 0);

        return boolArray;
    }
}
