using System.Xml.Linq;

namespace GameHook.Domain;

public static class MapperXmlExtensions
{
    static string ReplaceStart(this string input, string oldValue, string newValue)
    {
        if (input.StartsWith(oldValue))
        {
            return string.Concat(newValue, input.AsSpan(oldValue.Length));
        }
        else
        {
            return input;
        }
    }

    static string ReplaceEnd(this string input, string oldValue, string newValue)
    {
        if (input.EndsWith(oldValue))
        {
            return string.Concat(input.AsSpan(0, input.Length - oldValue.Length), newValue);
        }
        else
        {
            return input;
        }
    }

    public static string GetAttributeValue(this XElement el, string name) =>
        el.Attribute(name)?.Value ?? throw new Exception($"Node does not have required '{name}' attribute. {el}");

    public static string? GetOptionalAttributeValue(this XElement el, string name) =>
        el.Attribute(name)?.Value;

    public static int? GetOptionalAttributeValueAsInt(this XElement el, string name) =>
        el.Attribute(name) != null ? int.Parse(el.GetAttributeValue(name)) : null;

    public static bool IsArray(this XElement el)
    {
        var childElements = el.Elements().Select(x => x.GetOptionalAttributeValue("name") ?? string.Empty).ToArray();

        // Check if all child elements are numbers
        if (childElements.Any(e => int.TryParse(e, out _)) == false) {
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

    public static bool IsParentAnArray(this XElement el)
    {
        return el.Parent?.IsArray() ?? false;
    }
    
    public static string? GetElementActualName(this XElement el)
    {
        if (el.Name.LocalName is "property" || el.Name.LocalName is "class")
        {
            return el.GetAttributeValue("name").Replace("-", string.Empty);
        }
        else
        {
            return el.Name.LocalName.Replace("-", string.Empty);
        }
    }

    static string? GetElementPathName(this XElement el)
    {
        if (el.Name.LocalName is "class")
        {
            return el.GetAttributeValue("name");
        }
        else
        {
            return el.Name.LocalName;
        }
    }

    public static string GetElementPath(this XElement el)
    {
        var elementName = el.Attribute("name")?.Value;
        
        return el
                   .AncestorsAndSelf().InDocumentOrder().Reverse()
                   .Aggregate("", (s, xe) => xe.GetElementPathName() + "." + s)
                   .ReplaceStart("mapper.properties.", string.Empty).ReplaceEnd(".property.", string.Empty) +
               $".{elementName}";
    }
}