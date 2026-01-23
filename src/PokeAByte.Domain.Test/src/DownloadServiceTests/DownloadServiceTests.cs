namespace PokeAByte.Domain.Test.MapperServiceTests;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PokeAByte.Domain.Models.Mappers;
using RichardSzalay.MockHttp;
using Voyager.UnitTestLogger;
using Xunit;

public class DownloadServiceTests : MapperTestBase
{
    [Fact]
    public async Task FetchesLatestCommit()
    {
        var mockHttp = new MockHttpMessageHandler();
        var commitLookupRequest = mockHttp
            .Expect("https://api.github.com/repos/PokeAByte/mappers/commits/main")
            .WithHeaders([
                new KeyValuePair<string, string>("User-Agent", "Poke-A-Byte"),
                new KeyValuePair<string, string>("Accept", "application/vnd.github+json")
            ])
            .Respond(
                "application/json",
                """
                {
                    "sha": "c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1",
                    "node_id": "C_kwDOMl8cYdoAKGMzN2MzY2U5MzFlZTlmMjQ1ZTQxYjJmZTU3MjJmNmQxZWQ5YWZkZDE",
                    "url": "https://api.github.com/repos/PokeAByte/mappers/commits/c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1"
                }
                """
            );

        var service = new DownloadService(
            new SpyLog<DownloadService>(),
            new TestClientNotifier(),
            new HttpClient(mockHttp)
        );
        var update = service.GetLatestUpdate();

        Assert.Equal(1, mockHttp.GetMatchCount(commitLookupRequest));
        mockHttp.VerifyNoOutstandingExpectation();
        Assert.NotNull(update);
        Assert.Equal("c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1", update.Hash);

        // Check that a second call does nothing because of the rate limiting / caching:
        var secondUpdate = service.GetLatestUpdate();
        Assert.Equal(update, secondUpdate);
        Assert.Equal(1, mockHttp.GetMatchCount(commitLookupRequest));
    }

    [Fact]
    public async Task FetchesCommitWithAuthentication()
    {
        var mockHttp = new MockHttpMessageHandler();
        File.WriteAllText("./github_api_settings.json", """{ "token": "9jg37l6ytcv3yrq9ti9jpcrd01de5eux" }""");
        var commitLookupRequest = mockHttp
            .Expect("https://api.github.com/repos/PokeAByte/mappers/commits/main")
            .WithHeaders("Authorization", "Bearer 9jg37l6ytcv3yrq9ti9jpcrd01de5eux")
            .Respond(
                "application/json",
                """
                {
                    "sha": "c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1",
                    "node_id": "C_kwDOMl8cYdoAKGMzN2MzY2U5MzFlZTlmMjQ1ZTQxYjJmZTU3MjJmNmQxZWQ5YWZkZDE",
                    "url": "https://api.github.com/repos/PokeAByte/mappers/commits/c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1"
                }
                """
            );

        var service = new DownloadService(
            new SpyLog<DownloadService>(),
            new TestClientNotifier(),
            new HttpClient(mockHttp)
        );
        var update = service.GetLatestUpdate();

        // Assert.Equal(1, mockHttp.GetMatchCount(commitLookupRequest));
        mockHttp.VerifyNoOutstandingExpectation();
        Assert.NotNull(update);
        Assert.Equal("c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1", update.Hash);

        // Check that a second call does nothing because of the rate limiting / caching:
        var secondUpdate = service.GetLatestUpdate();
        Assert.Equal(update, secondUpdate);
        Assert.Equal(1, mockHttp.GetMatchCount(commitLookupRequest));
    }

    [Fact]
    public async Task FetchesMapperTree()
    {
        CreateLastFetchFile();
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp
            .Expect("https://cdn.jsdelivr.net/gh/PokeAByte/mappers@c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1/mapper_tree.json")
            .Respond(
                "application/json",
                """
                [
                    {"display_name": "Test mapper", "path": "test/test_1.xml", "version": "1.0.0" },
                    {"display_name": "Test mapper 2", "path": "test/test_2.xml", "version": "2.0.0" }
                ]
                """
            );

        var service = new DownloadService(
            new SpyLog<DownloadService>(),
            new TestClientNotifier(),
            new HttpClient(mockHttp)
        );

        var result = await service.FetchMapperTree();
        mockHttp.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(new MapperFile("Test mapper", "test/test_1.xml", "1.0.0"), result.First());
        Assert.Equal(new MapperFile("Test mapper 2", "test/test_2.xml", "2.0.0"), result.Last());
        Assert.Equal(1, mockHttp.GetMatchCount(request));
    }

    [Fact]
    public async Task FetchesMapper()
    {
        CreateLastFetchFile();
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp
            .Expect("https://cdn.jsdelivr.net/gh/PokeAByte/mappers@c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1/test/test_1.xml")
            .Respond(
                "application/json",
                """
                <mapper name="Test 1" version="1.0.0" />
                """
            );
        mockHttp
            .When("https://cdn.jsdelivr.net/gh/PokeAByte/mappers@c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1/test/test_1.js")
            .Respond(
                "application/json",
                "null"
            );

        var service = new DownloadService(
            new SpyLog<DownloadService>(),
            new TestClientNotifier(),
            new HttpClient(mockHttp)
        );

        var result = await service.DownloadMapperAsync("test/test_1.xml");
        mockHttp.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        Assert.Equal("""<mapper name="Test 1" version="1.0.0" />""", result.XmlData);
        Assert.Equal(1, mockHttp.GetMatchCount(request));
    }

    private void CreateLastFetchFile()
    {
        File.WriteAllText(
            "./Mappers/last_fetch.json",
            """{"Hash": "c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1", "LastTry": "2026-03-04T16:32:33.6103477+00:00"}"""
        );
    }

    [Fact]
    public async Task DownloadsWithGithubSettings()
    {
        CreateLastFetchFile();
        File.WriteAllText("./github_api_settings.json", """{ "owner": "ForkAByte", "repo": "Mappers" }""");
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp
            .Expect("https://cdn.jsdelivr.net/gh/ForkAByte/Mappers@c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1/test/test_1.xml")
            .Respond("application/json", """<mapper name="Test 1" version="1.0.0" />""");
        mockHttp
            .Expect("https://cdn.jsdelivr.net/gh/ForkAByte/Mappers@c37c3ce931ee9f245e41b2fe5722f6d1ed9afdd1/test/test_1.js")
            .Respond("application/json", "null");

        var service = new DownloadService(
            new SpyLog<DownloadService>(),
            new TestClientNotifier(),
            new HttpClient(mockHttp)
        );

        var result = await service.DownloadMapperAsync("test/test_1.xml");
        mockHttp.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        Assert.Equal("""<mapper name="Test 1" version="1.0.0" />""", result.XmlData);
        Assert.Equal(1, mockHttp.GetMatchCount(request));
    }

    [Fact]
    public async Task SavesGithubSettings()
    {
        CreateLastFetchFile();
        Assert.False(File.Exists("./github_api_settings.json"));

        var mockHttp = new MockHttpMessageHandler();

        var service = new DownloadService(
            new SpyLog<DownloadService>(),
            new TestClientNotifier(),
            new HttpClient(mockHttp)
        );

        service.UpdateApiSettings(
            new DownloadSettings()
            {
                Owner = "ForkAByte",
                Repo = "Mappers",
            }
        );
        mockHttp.VerifyNoOutstandingExpectation();
        Assert.True(File.Exists("./github_api_settings.json"));
        Assert.Equal(
            "{\n  \"token\": \"\",\n  \"owner\": \"ForkAByte\",\n  \"repo\": \"Mappers\",\n  \"dir\": \"\"\n}",
            File.ReadAllText("./github_api_settings.json")
        );
    }
}