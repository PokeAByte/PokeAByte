namespace PokeAByte.Domain.Interfaces;

public record MapperDownloadDto(
    string RelativeXmlPath,
    string XmlData,
    string RelativeJsPath,
    string JsData
);
