using PokeAByte.Domain.Models.Mappers;

namespace PokeAByte.Domain.Interfaces;

public interface IDownloadService
{
    DownloadSettings Settings { get; set; }
    bool ChangesAvailable();
    Task<MapperDownloadDto?> DownloadMapperAsync(string path);
    Task<List<MapperFile>?> FetchMapperTree();
    GithubUpdate? GetLatestUpdate(bool force = false);
    Task<bool> TestSettings();
    void UpdateApiSettings(DownloadSettings settings);
}