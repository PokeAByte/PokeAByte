using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.PokeAByteProperties;

namespace PokeAByte.Domain;

public class BooleanProperty : PokeAByteProperty, IPokeAByteProperty
{
    public BooleanProperty(IPokeAByteInstance instance, PropertyAttributes variables) : base(instance, variables)
    {
    }

    protected override byte[] FromValue(string value)
    {
        var booleanValue = bool.Parse(value);
        return booleanValue == true ? [0x01] : [0x00];
    }

    protected override object? ToValue(byte[] data)
    {
        return data[0] != 0x00;
    }
}

