using PokeAByte.Domain.Models.Mappers;

namespace PokeAByte.Domain.Interfaces;

public interface IGithubRestApi
{
    public Task DownloadMapperFiles(
        List<MapperDto> mapperDtos,
        Func<List<UpdateMapperDto>, Task> postDownloadAction
    );

    public Task<HttpContent?> GetMapperTreeFile();
    public Task<HttpResponseMessage?> GetContentRequest(string? path = null, bool isFile = false);
    public Task<string> TestSettings();
}