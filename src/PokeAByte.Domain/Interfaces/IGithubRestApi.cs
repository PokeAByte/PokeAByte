using PokeAByte.Domain.Models.Mappers;

namespace PokeAByte.Domain.Interfaces;

public interface IGithubService
{
    Task<List<UpdateMapperDto>> DownloadMappersAsync(List<MapperDto> mapperDtos);
    Task<bool> TestSettings();
    Task<HttpContent?> GetMapperTreeFile();
    void ApplySettings(IGithubSettings settings);
    public IGithubSettings Settings { get; }
}
