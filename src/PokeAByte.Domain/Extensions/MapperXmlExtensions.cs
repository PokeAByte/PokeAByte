using System.Xml.Linq;

namespace PokeAByte.Domain;

public static class MapperXmlExtensions
{
    static string ReplaceStart(this string input, string oldValue, string newValue)
    {
        return input.StartsWith(oldValue)
            ? string.Concat(newValue, input.AsSpan(oldValue.Length)) 
            : input;
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

    static string? GetElementPathName(this XElement el)
    {
        return el.Name.LocalName is "class" 
            ? el.GetAttributeValue("name") 
            : el.Name.LocalName;
    }

    public static string GetElementPath(this XElement element)
    {
        var elementName = element.Attribute("name")?.Value;
        return element
            .AncestorsAndSelf()
            .InDocumentOrder()
            .Reverse()
            .Aggregate("", (s, xe) => xe.GetElementPathName() + "." + s)
            .ReplaceStart("mapper.properties.", string.Empty)
            .ReplaceEnd(".property.", string.Empty)
            + $".{elementName}";
    }
}