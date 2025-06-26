using System.Xml.Linq;
using PokeAByte.Domain;

namespace PokeAByte.Utility.BuildMapperBindings;

public static class Extensions
{
    public static string CapitalizeFirstLetter(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input; // Return input as it is, if it's null or empty
        }
        return char.ToUpper(input[0]) + input[1..];
    }

    public static bool IsParentAnArray(this XElement el)
    {
        return el.Parent?.IsArray() ?? false;
    }

    public static bool IsArray(this XElement el)
    {
        var childElements = el.Elements().Select(x => x.GetOptionalAttributeValue("name") ?? string.Empty).ToArray();

        // Check if all child elements are numbers
        if (childElements.Any(e => !int.TryParse(e, out _)))
        {
            return false;
        }

        // Check if numbers are in sequence
        var sortedElements = childElements.OrderBy(e => int.Parse(e)).ToList();
        for (var i = 1; i < sortedElements.Count; i++)
        {
            var current = int.Parse(sortedElements[i]);
            var previous = int.Parse(sortedElements[i - 1]);

            if (current != previous + 1) return false;
        }

        return true;
    }
}