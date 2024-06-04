namespace GameHook.Domain.Interfaces
{
    public record UpdateMapperDto(string RelativeXmlPath, string XmlData, string RelativeJsPath, string JsData);
    public interface IMapperUpdateManager
    {
        Task<bool> CheckForUpdates();
        Task SaveUpdatedMappersAsync(List<UpdateMapperDto> updatedMappers);
    }
}
