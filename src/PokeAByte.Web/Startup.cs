using System.Reflection;
using Microsoft.Extensions.FileProviders;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Logic;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Services.Mapper;
using PokeAByte.Domain.Services.MapperFile;
using PokeAByte.Infrastructure.Drivers;
using PokeAByte.Infrastructure.Github;
using PokeAByte.Web.ClientNotifiers;
using PokeAByte.Web.Controllers;
using PokeAByte.Web.Hubs;
using PokeAByte.Web.Middleware;
using PokeAByte.Web.Services.Drivers;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web;

public static class Startup
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddCors();

        services.AddSignalR()
            .AddJsonProtocol(
                (options) =>
                {
                    options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, ApiJsonContext.Default);
                    options.PayloadSerializerOptions.Converters.Add(new ByteArrayJsonConverter());
                }
            );
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Add(ApiJsonContext.Default);
            options.SerializerOptions.Converters.Add(new ByteArrayJsonConverter());
        });

        services.AddSingleton<AppSettingsService>();
        services.AddSingleton<ScriptConsole>();
        services.AddSingleton<IMapperFileService, MapperFileService>();
        services.AddSingleton<IStaticMemoryDriver, StaticMemoryDriver>();
        services.AddSingleton<IClientNotifier, WebSocketClientNotifier>();
        services.AddSingleton<IInstanceService, InstanceService>();
        services.AddSingleton<IDriverService, DriverService>();
        services.AddSingleton<RequestLogMiddleware>();

        services.AddScoped<IGithubService, GitHubService>();
        services.AddScoped(x =>
            {
                var logger = x.GetRequiredService<ILogger<MapperUpdaterSettings>>();
                return MapperUpdaterSettings.Load(logger);
            });
        services.AddScoped<IMapperUpdateManager, MapperUpdateManager>();
        services.AddScoped<MapperClientService>();
    }

    public static void ConfigureApp(this WebApplication app)
    {
        Directory.CreateDirectory(BuildEnvironment.ConfigurationDirectory);
        // Configure the HTTP request pipeline.
        app.UseCors(x =>
        {
            x.SetIsOriginAllowed(x => true);
            x.AllowAnyMethod();
            x.AllowAnyHeader();
            x.AllowCredentials();
        });

        var embeddedFileProvider = new ManifestEmbeddedFileProvider(Assembly.GetEntryAssembly()!);
        app.UseSpaStaticFiles(new() { FileProvider = embeddedFileProvider });
        app.UseSpa(configuration =>
        {
            configuration.Options.DefaultPageStaticFileOptions = new() { FileProvider = embeddedFileProvider };
            configuration.Options.DefaultPage = "/index.html";
            configuration.Options.DefaultPageStaticFileOptions?.OnPrepareResponse = (context) =>
            {
                context.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
                context.Context.Response.Headers.Append("X-Clacks-Overhead", "GNU Terry Pratchett");
                context.Context.Response.Headers.Append("Expires", "-1");
            };
        });
        app.UseMiddleware<RequestLogMiddleware>();
        app.MapHub<UpdateHub>("/updates");

        app.MapFilesEndpoints();
        app.MapGithubEndpoints();
        app.MapDriverEndpoints();
        app.MapMapperEndpoints();
        app.MapMapperServiceEndpoints();
        app.MapSettingsEndpoints();

        app.MapGet("/favicon.png", () => Results.File(ApiHelper.EmbededResources.Favicon, contentType: "image/png"));
        app.MapGet("/dist/gameHookMapperClient.js", () => Results.File(ApiHelper.EmbededResources.ClientScript, contentType: "application/javascript"));
    }
}