using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Infrastructure.Github;

namespace PokeAByte.Web;

public static class GithubEndpoints
{
    public static void MapGithubEndpoints(this WebApplication app)
    {
        app.MapPost("/files/save_github_settings", SaveGithubSettingsAsync);
        app.MapGet("/files/get_github_settings", (IGithubService githubService) =>
        {
            return githubService.Settings;
        });
        app.MapGet("/files/test_github_settings", TestGithubSettingsAsync);
        app.MapGet("/files/get_github_link", (IGithubService githubService) => githubService.Settings.GetGithubUrl());
    }

    public static IResult SaveGithubSettingsAsync(
        [FromServices] IGithubService githubService,
        [FromBody] GithubSettings settings)
    {
        // await FilesEndpoints.CheckForUpdatesAsync(mapperFileService, updateManager);
        githubService.ApplySettings(settings);
        return TypedResults.Ok();
    }

    public static async Task<IResult> TestGithubSettingsAsync(IGithubService githubService)
    {
        bool result = await githubService.TestSettings();
        return result
            ? TypedResults.Ok("Successfully connected to Github Api!")
            : TypedResults.Ok($"Failed to connect to Github Api - Reason: {result}");
    }
}
