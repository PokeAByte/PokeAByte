using System.Xml;
using System.Xml.Linq;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Logic;
using PokeAByte.Domain.Plantforms;
using PokeAByte.Domain.PokeAByteProperties;

namespace PokeAByte.Domain.Mapper;

static class PokeAByteMapperXmlHelpers
{
    public static IEnumerable<XAttribute> GetAttributesWithVars(this XDocument doc)
    {
        var properties = doc.Descendants("properties") ?? throw new Exception("Unable to locate <properties>");

        return properties
            .Descendants()
            .Attributes()
            .Where(x => x.Name.NamespaceName == "https://schemas.pokeabyte.io/attributes/var");
    }

    static string[] AttributesThatCanBeNormalized { get; } = ["address", "preprocessor"];
    public static List<XAttribute> GetAttributesThatCanBeNormalized(this XDocument doc)
    {
        var properties = doc.Descendants("properties") ?? throw new Exception("Unable to locate <properties>");

        return properties
            .Descendants()
            .Attributes()
            .Where(x => AttributesThatCanBeNormalized.Contains(x.Name.LocalName))
            .Where(x => x.Value.Contains("0x"))
            .ToList();
    }
}

public static class PokeAByteMapperXmlFactory
{
    static ulong ToULong(string value)
    {
        try
        {
            if (value.StartsWith("0x"))
            {
                return Convert.ToUInt64(value, 16);
            }

            return Convert.ToUInt64(value);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to translate {value} into a ulong.", ex);
        }
    }

    public static MetadataSection GetMetadata(XDocument doc, string fileId)
    {
        var root = doc.Element("mapper") ?? throw new Exception($"Unable to find <mapper> root element.");

        return new MetadataSection()
        {
            Id = Guid.Parse(root.GetAttributeValue("id")),
            FileId = fileId,
            GameName = root.GetAttributeValue("name"),
            GamePlatform = root.GetAttributeValue("platform")
        };
    }

    public static MemorySection GetMemory(XDocument doc)
    {
        var memory = doc.Element("mapper")?.Element("memory");

        if (memory == null) { return new MemorySection(); }

        return new MemorySection()
        {
            ReadRanges = memory.Elements("read").Select(x => new ReadRange()
            {
                Start = x.GetAttributeValue("start").ParseHexAddress(),
                End = x.GetAttributeValue("end").ParseHexAddress(),
            }).ToArray()
        };
    }

    static IEnumerable<IPokeAByteProperty> GetProperties(XDocument doc, EndianTypes endianType)
    {
        return doc.Descendants("properties").Descendants("property")
            .Select<XElement, IPokeAByteProperty>(x =>
            {
                try
                {
                    var type = x.GetAttributeValue("type") switch
                    {
                        "binaryCodedDecimal" => PropertyType.BinaryCodedDecimal,
                        "bitArray" => PropertyType.BitArray,
                        "bool" => PropertyType.Bool,
                        "bit" => PropertyType.Bool,
                        "int" => PropertyType.Int,
                        "string" => PropertyType.String,
                        "uint" => PropertyType.Uint,
                        _ => throw new Exception($"Unknown property type {x.GetAttributeValue("type")}."),
                    };

                    var variables = new PropertyAttributes()
                    {
                        Path = x.GetElementPath(),
                        Type = type,
                        MemoryContainer = x.GetOptionalAttributeValue("memoryContainer"),
                        Address = x.GetOptionalAttributeValue("address"),
                        Length = x.GetOptionalAttributeValueAsInt("length") ?? 1,
                        Size = x.GetOptionalAttributeValueAsInt("size"),
                        Bits = x.GetOptionalAttributeValue("bits"),
                        Reference = x.GetOptionalAttributeValue("reference"),
                        Description = x.GetOptionalAttributeValue("description"),
                        Value = x.GetOptionalAttributeValue("value"),
                        ReadFunction = x.GetOptionalAttributeValue("read-function"),
                        WriteFunction = x.GetOptionalAttributeValue("write-function"),
                        AfterReadValueExpression = x.GetOptionalAttributeValue("after-read-value-expression"),
                        AfterReadValueFunction = x.GetOptionalAttributeValue("after-read-value-function"),
                        BeforeWriteValueFunction = x.GetOptionalAttributeValue("before-write-value-function"),
                        EndianType = endianType,
                    };

                    return new PokeAByteProperty(variables);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to parse {x.GetElementPath()}. {x}", innerException: ex);
                }
            })
            .ToArray();
    }

    public static IEnumerable<ReferenceItems> GetGlossary(XDocument doc)
    {
        return doc.Descendants("references")
            .Elements()
            .Select(el =>
            {
                var name = el.Name.LocalName;
                var type = el.GetOptionalAttributeValue("type") ?? "string";

                return new ReferenceItems()
                {
                    Name = name,
                    Type = type,
                    Values = el.Elements().Select(y =>
                    {
                        var key = ToULong(y.GetAttributeValue("key"));
                        object? value = null;

                        var valueStr = y.GetOptionalAttributeValue("value");
                        if (string.IsNullOrEmpty(valueStr)) { value = null; }
                        else if (type == "string") { value = valueStr; }
                        else if (type == "number") { value = int.Parse(valueStr); }
                        else throw new Exception($"Unknown type for reference list {type}.");

                        return new ReferenceItem(key, value);
                    }).ToArray()
                };
            });
    }

    public static IPokeAByteMapper LoadMapperFromFile(string mapperContents, string fileId)
    {
        var doc = XDocument.Parse(mapperContents.Replace("{", string.Empty).Replace("}", string.Empty));

        // Apply Macros
        var destinationMacros = doc.Descendants("macro").ToArray();
        foreach (var destinationMacro in destinationMacros)
        {
            var macroType = destinationMacro.GetAttributeValue("type");
            var sourceMacro = doc.Descendants("macros").Descendants(macroType).FirstOrDefault() ??
                                throw new XmlException($"Unable to find macro in <macros> tag of {macroType}.");

            destinationMacro.ReplaceWith(sourceMacro.Elements());
        }

        // Apply Classes.
        var destinationClasses = doc.Descendants("properties").Descendants("class");
        foreach (var destinationClass in destinationClasses)
        {
            var classType = destinationClass.GetAttributeValue("type");
            var sourceClass = doc.Descendants("classes").Descendants(classType).FirstOrDefault() ??
                                throw new XmlException($"Unable to find class in <classes> tag of {classType}.");

            destinationClass.ReplaceNodes(sourceClass.Elements());
        }

        // Apply static variable replacement.
        var attributesWithVars = doc.GetAttributesWithVars();
        foreach (var attr in attributesWithVars)
        {
            if (attr.Parent == null) throw new Exception($"Cannot get parent from attribute {attr}.");

            var varName = attr.Name.LocalName;
            var varValue = attr.Value;

            foreach (var attribute in attr.Parent.Descendants().Attributes())
            {
                if (attribute.Name.LocalName == "name") { continue; }
                if (attribute.Name.LocalName == "type") { continue; }

                attribute.Value = attribute.Value.Replace(varName, varValue);
            }
        }

        // Apply normalization of hexdecimals.
        foreach (var attr in doc.GetAttributesThatCanBeNormalized())
        {
            attr.Value = attr.Value.NormalizeMemoryAddresses();
        }
        var metaData = GetMetadata(doc, fileId);
        IPlatformOptions platformOptions = metaData.GamePlatform switch
        {
            "NES" => new NES_PlatformOptions(),
            "SNES" => new SNES_PlatformOptions(),
            "GB" => new GB_PlatformOptions(),
            "GBC" => new GBC_PlatformOptions(),
            "GBA" => new GBA_PlatformOptions(),
            "PSX" => new PSX_PlatformOptions(),
            "NDS" => new NDS_PlatformOptions(),
            _ => throw new Exception($"Unknown game platform {metaData.GamePlatform}.")
        };
        return new PokeAByteMapper(
            metaData,
            platformOptions,
            GetMemory(doc),
            GetProperties(doc, platformOptions.EndianType),
            GetGlossary(doc)
        );
    }
}
