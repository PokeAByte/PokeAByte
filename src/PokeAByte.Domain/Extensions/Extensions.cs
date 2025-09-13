using System.Globalization;
using System.Text.RegularExpressions;

namespace PokeAByte.Domain;

public static partial class Extensions
{
    [GeneratedRegex(@"0x[a-fA-F\d]+")]
    private static partial Regex HexdecimalMemoryAddress();
    public static string ToHexdecimalString(this MemoryAddress value) => $"0x{value:X2}";
    public static string ToHexdecimalString(this byte value) => ((uint)value).ToHexdecimalString();
    
    /// <summary>
    /// Parse a hex memory address into a uint. 
    /// </summary>
    /// <param name="value"> The hex address (e.g 0xD16A) </param>
    /// <returns> The parsed uint. </returns>
    /// <exception cref="Exception"></exception>
    public static uint ParseHexAddress(this string value)
    {
        var span = value.AsSpan();
        if (span.StartsWith("0x"))
        {
            span = span[2..];
        }
        try
        {
            return uint.Parse(span, NumberStyles.HexNumber);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to translate {value} into a memory address / uint32.", ex);
        }
    }

    /// <summary>
    /// Normalize memory address designations by resolving hexadecimal addressed into decimal. 
    /// For example <c>0xD16A + 33</c> => <c>53610 + 33</c>
    /// </summary>
    /// <param name="value"> The address to normalize. </param>
    /// <returns> The normalized address designation. </returns>
    /// <exception cref="Exception"> When the hex address can not be parsed. </exception>
    public static string NormalizeMemoryAddresses(this string value)
    {
        if (value.Contains("0x"))
        {
            return HexdecimalMemoryAddress().Replace(value, match =>
            {
                try
                {
                    return uint.Parse(match.Value[2..], NumberStyles.HexNumber).ToString();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to translate {match.Value} into a memory address / uint32.", ex);
                }
            });
        }

        return value;
    }

    public static string ToPascalCase(this string value)
    {
        string[] words = Regex.Split(value, @"[_\-]");
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            if (!string.IsNullOrEmpty(word))
            {
                words[i] = textInfo.ToTitleCase(word);
            }
        }

        return string.Concat(words);
    }
}
