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
}