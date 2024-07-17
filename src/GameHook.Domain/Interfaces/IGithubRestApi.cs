using GameHook.Domain.Models;
using GameHook.Domain.Models.Mappers;

namespace GameHook.Domain.Interfaces;

public interface IGithubRestApi
{
    public Task DownloadMapperFiles(List<MapperDto> mapperDtos,
        Func<List<UpdateMapperDto>, Task> postDownloadAction,
        Action<int>? currentProcessCountUpdate = null);
    public Task<HttpResponseMessage?> GetMapperTreeFile();
    public Task<HttpResponseMessage?> GetContentRequest(string? path = null, bool isFile = false);
    public Task<string> TestSettings();
}