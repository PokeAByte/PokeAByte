using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Infrastructure.Github;

namespace PokeAByte.Web;

public static class GithubEndpoints
{
    public static void MapGithubEndpoints(this WebApplication app)
    {
        app.MapPost("/files/save_github_settings", SaveGithubSettingsAsync);
        app.MapGet("/files/get_github_settings", (IGithubApiSettings githubApiSettings) => (GithubApiSettings)githubApiSettings);
        app.MapGet("/files/test_github_settings", TestGithubSettingsAsync);
        app.MapGet("/files/get_github_link", (IGithubApiSettings settings) => settings.GetGithubUrl());
    }

    public static async Task<IResult> SaveGithubSettingsAsync(
        IMapperUpdateManager updateManager,
        IGithubApiSettings githubApiSettings,
        ILogger<RestAPI> logger,
        [FromBody] GithubApiSettings settings)
    {
        await FilesEndpoints.CheckForUpdatesAsync(updateManager);
        githubApiSettings.CopySettings(settings);
        githubApiSettings.SaveChanges();
        return TypedResults.Ok();
    }

    public static async Task<IResult> TestGithubSettingsAsync(IGithubRestApi githubRest)
    {
        var result = await githubRest.TestSettings();
        return string.IsNullOrWhiteSpace(result)
            ? TypedResults.Ok("Successfully connected to Github Api!")
            : TypedResults.Ok($"Failed to connect to Github Api - Reason: {result}");
    }
}
