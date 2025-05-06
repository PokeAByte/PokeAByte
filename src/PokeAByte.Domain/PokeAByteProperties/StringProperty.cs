using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.PokeAByteProperties;

public class StringProperty : PokeAByteProperty, IPokeAByteProperty
{
    public StringProperty(IPokeAByteInstance instance, PropertyAttributes variables) : base(instance, variables)
    {
        if (Reference == null)
        {
            Reference = "defaultCharacterMap";
        }
    }

    protected override byte[] FromValue(string value)
    {
        if (ComputedReference == null) throw new Exception("ReferenceObject is NULL.");
        if (Length == null) throw new Exception("Length is NULL.");

        var uints = value
            .Select(x => ComputedReference.Values.FirstOrDefault(y => x.ToString() == y?.Value?.ToString()))
            .ToList();

        if (uints.Count + 1 > Length)
        {
            uints = uints.Take(Length ?? 0 - 1).ToList();
        }

        var nullTerminationKey = ComputedReference.Values.First(x => x.Value == null);
        uints.Add(nullTerminationKey);
        return uints
            .Select(x =>
            {
                return x?.Value == null
                    ? nullTerminationKey.Key
                    : x.Key;
            }).SelectMany(x =>
            {
                return Size is null
                    ? BitConverter.GetBytes(x).Take(new Range(0, 1))
                    : BitConverter.GetBytes(x).Take(Size.Value);
            })
            .ToArray();
    }

    protected override object? ToValue(byte[] data)
    {
        if (ComputedReference == null) { throw new Exception("ReferenceObject is NULL."); }

        string?[] results = [];
        if (Size > 1)
        {
            // For strings that have characters mapper to more than a single byte.
            results = data.Chunk(Size.Value).Select(b =>
            {
                var value = b.ReverseBytesIfBE(_endian).get_ulong_be();
                var referenceItem = ComputedReference.Values.SingleOrDefault(x => x.Key == value);
                return referenceItem?.Value?.ToString() ?? null;
            }).ToArray();
            // Return the completed string buffer.
            return string.Concat(results.TakeWhile(s => s != null));
        }

        // For strings where one character equals one byte.
        results = new string?[data.Length];
        int i = 0;
        for (; i < data.Length; i++)
        {
            var referenceItem = ComputedReference.Values.SingleOrDefault(x => x.Key == data[i]);
            if (referenceItem?.Value == null)
            {
                break;
            }
            results[i] = referenceItem.Value.ToString();
        }
        return string.Concat(results[0..i]);
    }
}
