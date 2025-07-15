namespace PokeAByte.Domain.Interfaces;

public record UpdateMapperDto(
    string RelativeXmlPath,
    string XmlData,
    string RelativeJsPath,
    string JsData,
    DateTime Created,
    DateTime? Updated
);

public interface IMapperUpdateManager
{
    Task<bool> CheckForUpdates();
    Task SaveUpdatedMappersAsync(List<UpdateMapperDto> mappers);
}
