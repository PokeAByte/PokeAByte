using System.Xml.Linq;

namespace PokeAByte.Domain;

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