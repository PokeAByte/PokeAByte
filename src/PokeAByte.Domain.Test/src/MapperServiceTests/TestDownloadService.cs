
namespace PokeAByte.Domain.Test.MapperServiceTests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Mappers;

public class TestDownloadService : IDownloadService
{
    public DownloadSettings Settings { get; set; }

    public bool ChangesAvailable()
    {
        return true;
    }

    public async Task<MapperDownloadDto> DownloadMapperAsync(string path)
    {
        if (path == "test/xml_only.xml")
        {
            return new MapperDownloadDto(path, """<mapper name="xml_only" />""", "", "");
        }
        if (path == "test/xml_script.xml")
        {
            return new MapperDownloadDto(
                path,
                """<mapper name="xml_script" />""",
                "./test/xml_script.js",
                "export function preprocessor() { return false; }"
            );
        }
        throw new Exception("Wrong mapper path.");
    }

    public async Task<List<MapperFile>> FetchMapperTree()
    {
        return [
            new MapperFile("Test mapper", "test/xml_only.xml", "1.0.0"),
            new MapperFile("Test mapper 2", "test/xml_script.xml", "1.0.0"),
        ];
    }

    public GithubUpdate GetLatestUpdate(bool force = false)
    {
        return new GithubUpdate("hash", System.DateTimeOffset.UtcNow);
    }

    public Task<bool> TestSettings()
    {
        return Task.FromResult(true);
    }

    public void UpdateApiSettings(DownloadSettings settings)
    {
        return;
    }
}