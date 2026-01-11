using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Web;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this WebApplication app)
    {
        app.MapPost("/settings/save_appsettings", SaveAppSettings);
        app.MapPost("/settings/appsettings/reset", ResetAppSettings);
        app.MapGet("/settings/appsettings", (AppSettingsService provider) => provider.Get());
    }

    public static IResult ResetAppSettings(
        [FromServices] IGithubService githubService,
        [FromServices] AppSettingsService provider)
    {
        provider.Set(new AppSettings());
        provider.Save();
        return TypedResults.Ok();
    }

    public static IResult SaveAppSettings(
        [FromServices] IGithubService githubService,
        [FromServices] AppSettingsService provider,
        [FromBody] AppSettings newSettings)
    {
        provider.Set(newSettings);
        provider.Save();
        return TypedResults.Ok();
    }
}
