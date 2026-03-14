using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Web;

public static class GithubEndpoints
{
    public static void MapGithubEndpoints(this WebApplication app)
    {
        app.MapPost("/files/save_github_settings", SaveGithubSettingsAsync);
        app.MapGet("/files/get_github_settings", (IDownloadService downloadService) => downloadService.Settings);
        app.MapGet("/files/test_github_settings", TestGithubSettingsAsync);
        app.MapGet("/files/get_github_link", 
            (IDownloadService downloadService) => downloadService.Settings.GetGithubUrl()
        );
    }

    public static IResult SaveGithubSettingsAsync(
        [FromServices] IDownloadService downloadService,
        [FromBody] DownloadSettings settings)
    {
        downloadService.UpdateApiSettings(settings);
        return TypedResults.Ok();
    }

    public static async Task<IResult> TestGithubSettingsAsync(IDownloadService downloadService)
    {
        bool result = await downloadService.TestSettings();
        return result
            ? TypedResults.Ok("Successfully connected to Github Api!")
            : TypedResults.Ok($"Failed to connect to Github Api - Reason: {result}");
    }
}
