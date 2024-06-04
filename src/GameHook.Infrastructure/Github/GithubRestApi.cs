﻿using System.Net.Http.Headers;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameHook.Infrastructure.Github;

public class GithubRestApi
{
    private readonly GithubApiSettings _apiSettings;
    private readonly ILogger<GithubRestApi> _logger;
    
    public GithubRestApi(ILogger<GithubRestApi> logger,  
        GithubApiSettings apiSettings)
    {
        _logger = logger;
        _apiSettings = apiSettings;
    }
    private void UpdateRequestHeaders(HttpRequestHeaders headers)
    {
        if (string.IsNullOrWhiteSpace(_apiSettings.GetAcceptValue()) ||
            string.IsNullOrWhiteSpace(_apiSettings.GetApiVersionValue()))
        {
            //Failed to get the accept or api version strings, return a blank header and hope 
            //github rest api figures it out.
            _logger?.LogWarning("Accept or ApiVersion for the Github Api are null.");
            return;
        }
        headers.Add("Accept",_apiSettings.GetAcceptValue());
        headers.Add("X-GitHub-Api-Version", _apiSettings.GetApiVersionValue());
        if(!string.IsNullOrWhiteSpace(_apiSettings.GetTokenValue()))
            headers.Add("Authorization", _apiSettings.GetFormattedToken());
    }
    public async Task DownloadMapperFiles(List<MapperDto> mapperDtos, Func<List<UpdateMapperDto>, Task> 
        postDownloadAction)
    {
        List<UpdateMapperDto> updatedMapperList = new();
        foreach (var mapper in mapperDtos)
        {
            //Get the .xml and .js paths
            var xmlPath = mapper.Path;
            var jsPath = mapper.Path[..mapper.Path.IndexOf(".xml", StringComparison.Ordinal)] + ".js";
            //Get the xml data
            var xmlResponse = await GetContentRequest(xmlPath, true);
            var xmlData = await ResponseMessageToJson(xmlResponse);
            //Get the js data
            var jsResponse = await GetContentRequest(jsPath, true);
            var jsData = await ResponseMessageToJson(jsResponse);
            //Add them to the list
            updatedMapperList.Add(new UpdateMapperDto(xmlPath, xmlData ?? "", jsPath, jsData ?? ""));
            //Try to not process too fast, Github has a rate-limit 
            Thread.Sleep(10);
        }
        await postDownloadAction(updatedMapperList);
    }
    private async Task<string?> ResponseMessageToJson(HttpResponseMessage? responseMessage)
    {
        if (responseMessage is null || !responseMessage.IsSuccessStatusCode)
            return null;
        try
        {
            return await responseMessage.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to read message response.");
            return null;
        }
    }
    
    public async Task<HttpResponseMessage?> GetMapperTreeFile() =>
        await GetContentRequest(BuildEnvironment.MapperTreeJson, true);
    public async Task<HttpResponseMessage?> GetContentRequest(string? path = null, bool isFile = false)
    {
        var url = _apiSettings.GetBaseRequestString();
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogError("The Url generated for the GET request was null.");
            return null;
        }
        
        if (path is null && isFile is false)
        {
            _logger.LogInformation($"The path for sending a GET request is null, " +
                                    $"defaulting to {_apiSettings.GetDirectory()}.");
            path = _apiSettings.GetDirectory();
        }
        else if (path is null && isFile)
        {
            return null;
        }
        
        if (path != string.Empty)
        {
            url += $"/contents/{path}";
        }

        var clientRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
            Headers =
            {
                {"Accept", _apiSettings.GetAcceptValue()},
                {"X-GitHub-Api-Version", _apiSettings.GetApiVersionValue()},
                {"User-Agent", "request"}
            }
        };
        if (!string.IsNullOrWhiteSpace(_apiSettings.GetTokenValue()))
        {
            clientRequest.Headers.Add("Authorization",_apiSettings.GetFormattedToken());
        }
        //UpdateRequestHeaders(clientRequest.Headers);
        using var client = new HttpClient();
        return await client.SendAsync(clientRequest);
    }
}
