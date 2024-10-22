﻿using PokeAByte.Domain.Interfaces;

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
        /*var returnBytes = ulongArray
            .Select(x => (byte)x)
            .ToArray();*/
        return uints
            .Select(x =>
            {
                if (x?.Value == null)
                {
                    return nullTerminationKey.Key;
                }

                return x.Key;
            }).SelectMany(x =>
            {
                if (Size is null)
                    return BitConverter
                        .GetBytes(x)
                        .Take(new Range(0, 1));
                return BitConverter
                    .GetBytes(x)
                    .Take(Size ?? 1);
            })
            .ToArray();
    }

    protected override object? ToValue(byte[] data)
    {
        if (Instance == null) throw new Exception("Instance is NULL.");
        if (Instance.PlatformOptions == null) throw new Exception("Instance.PlatformOptions is NULL.");
        if (ComputedReference == null) { throw new Exception("ReferenceObject is NULL."); }

        string?[] results = Array.Empty<string>();

        if (Size > 1)
        {
            // For strings that have characters mapper to more than a single byte.
            results = data.Chunk(Size.Value).Select(b =>
            {
                var value = b.ReverseBytesIfBE(Instance.PlatformOptions.EndianType).get_ulong_be();

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
