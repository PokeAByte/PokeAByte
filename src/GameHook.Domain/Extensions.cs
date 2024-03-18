using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GameHook.Domain
{
    public static partial class Extensions
    {
        public static string ToHexdecimalString(this int value) => $"0x{value:X2}";
        public static string ToHexdecimalString(this MemoryAddress value) => $"0x{value:X2}";
        public static string ToHexdecimalString(this byte value) => ((uint)value).ToHexdecimalString();
        public static string ToHexdecimalString(this IEnumerable<int> value, string joinCharacter = " ") => string.Join(joinCharacter, value.Select(x => ((uint)x).ToHexdecimalString()));

        public static IEnumerable<int> ToIntegerArray(this byte[] bytes) => bytes.Select(x => (int)x).ToArray();

        public static string NormalizeMemoryAddresses(this string value)
        {
            if (value.Contains("0x"))
            {
                return HexdecimalMemoryAddress().Replace(value, match =>
                {
                    try
                    {
                        return Convert.ToUInt32(match.Value, 16).ToString();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Unable to translate {match.Value} into a memory address / uint32.", ex);
                    }
                });
            }

            return value;
        }

        public static string ToPascalCase(this string str)
        {
            string[] words = Regex.Split(str, @"[_\-]");
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (!string.IsNullOrEmpty(word))
                {
                    words[i] = textInfo.ToTitleCase(word);
                }
            }

            return string.Join("", words);
        }

        public static bool Between(this MemoryAddress value, MemoryAddress min, MemoryAddress max)
        {
            return value >= min && value <= max;
        }

        public static MemoryAddress ToMemoryAddress(this string memoryAddress)
        {
            if (MemoryAddress.TryParse(memoryAddress, out var result)) { return result; }
            throw new Exception($"Unable to determine memory address from string {memoryAddress}. It must be in decimal form (not hexdecimal).");
        }

        public static int GetIntParameterFromFunction(this string function, int position)
        {
            return int.Parse(function.Between("(", ")").Split(",")[position]);
        }

        public static MemoryAddress GetMemoryAddressFromFunction(this string function, int position)
        {
            return function.Between("(", ")").Split(",")[position].ToMemoryAddress();
        }

        public static string Between(this string str, string firstString, string lastString)
        {
            int start = str.IndexOf(firstString) + firstString.Length;
            int end = str.IndexOf(lastString);
            return str.Substring(start, end - start);
        }

        public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
        {
            foreach (var value in list) await func(value);
        }

        public static string CapitalizeFirstLetter(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input; // Return input as it is if it's null or empty
            }

            char firstChar = char.ToUpper(input[0]); // Convert the first character to uppercase
            string restOfString = input.Substring(1); // Get the remaining characters of the string

            return firstChar + restOfString;
        }

        [GeneratedRegex(@"0x[a-fA-F\d]+")]
        private static partial Regex HexdecimalMemoryAddress();
    }
}
