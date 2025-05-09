using System.Collections;
using System.Runtime.CompilerServices;

namespace PokeAByte.Domain;

internal static class PropertyLogic
{
    internal static int[] ParseBits(ReadOnlySpan<char> bits)
    {
        if (bits.Contains('-'))
        {
            var dashIndex = bits.IndexOf('-');
            var part1 = bits[..dashIndex];
            var part2 = bits[(dashIndex + 1)..];
            return int.TryParse(part1, out int start) && int.TryParse(part2, out int end)
                ? [.. Enumerable.Range(start, end - start + 1)]
                : throw new ArgumentException($"Invalid format for attribute Bits ({bits}).");
        }

        if (bits.Contains(','))
        {
            int[] indexes;
            indexes = new int[bits.Count(',') + 1];
            int x = 0;
            foreach (var range in bits.Split(','))
            {
                indexes[x++] = int.TryParse(bits[range], out int number)
                    ? number
                    : throw new ArgumentException($"Invalid format for attribute Bits ({bits}).");
            }
            return indexes;
        }

        return int.TryParse(bits, out int index)
            ? [index]
            : throw new ArgumentException($"Invalid format for attribute Bits ({bits}).");
    }

    // [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static int GetBCDValue(in byte[] data) 
    {
        int result = 0;
        foreach (byte bcd in data)
        {
            result *= 100;
            result += 10 * (bcd >> 4);
            result += bcd & 0xf;
        }
        return result;
    }

    // [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static bool[] GetBitArrayValue(in byte[] data) 
    {
        var bitArray = new BitArray(data);

        var boolArray = new bool[bitArray.Length];
        bitArray.CopyTo(boolArray, 0);
        return boolArray;                
    }

    // [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static int GetIntValue(in byte[] data, EndianTypes endian) 
    {
        if (data.Length == 1) //  With one byte, we can just cast.
        {
            return data[0];
        }
        // With less than 4 bytes, we have to pad the value before using the bitconverter:
        Span<byte> padded = stackalloc byte[4];
        data.AsSpan().CopyTo(padded);
        if (endian == EndianTypes.LittleEndian)
        {
            padded.Slice(0, data.Length).Reverse();
        }
        return BitConverter.ToInt32(padded);
    }

    // [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static uint GetUIntValue(in byte[] data, EndianTypes endian) 
    {
        // Shortcut: With one byte, we can just cast:
        if (data.Length == 1)
        {
            return (uint)data[0];
        }
        if (data.Length >= 4)
        {
            return BitConverter.ToUInt32(data.ReverseBytesIfLE(endian), 0);
        }
        // With less than 4 bytes, we have to pad the value before using the bitconverter:
        Span<byte> value = stackalloc byte[4];
        data.ReverseBytesIfLE(endian).AsSpan().CopyTo(value);
        return BitConverter.ToUInt32(value);
    }

    // [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static string GetStringValue(in byte[] data, int? Size, ReferenceItems? reference, EndianTypes _endian) 
    {
        if (reference == null) { throw new Exception("ReferenceObject is NULL."); }

        string?[] results = [];
        if (Size > 1)
        {
            // For strings that have characters mapper to more than a single byte.
            results = data.Chunk(Size.Value).Select(b =>
            {
                var value = b.ReverseBytesIfBE(_endian).get_ulong_be();
                var referenceItem = reference.Values.SingleOrDefault(x => x.Key == value);
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
            var key = data[i];
            var referenceItem = reference.Values.SingleOrDefault(x => x.Key == key);
            if (referenceItem?.Value == null)
            {
                break;
            }
            results[i] = referenceItem.Value.ToString();
        }
        return string.Concat(results[0..i]);
    }
}