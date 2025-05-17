using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
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
using PokeAByte.Web.Services.Drivers;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web;

public static class Startup
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        //Inherited services from GameHook
        services.AddCors();
        // Add Web API

        services.AddSignalR()
            .AddJsonProtocol(
                (options) =>
                {
                    options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, ApiJsonContext.Default);
                }
            );
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApiJsonContext.Default);
        });

        services.AddSingleton<AppSettings>();
        services.AddSingleton<ScriptConsole>();
        services.AddSingleton<IMapperFileService, MapperFileService>();
        services.AddSingleton<IStaticMemoryDriver, StaticMemoryDriver>();
        services.AddSingleton<IClientNotifier, WebSocketClientNotifier>();
        services.AddSingleton<IInstanceService, InstanceService>();
        services.AddSingleton<IDriverService, DriverService>();

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

        var provider = new ManifestEmbeddedFileProvider(Assembly.GetEntryAssembly()!);
        app.UseSpaStaticFiles(new() { FileProvider = provider });
        app.UseSpa(configuration =>
        {
            configuration.Options.DefaultPageStaticFileOptions = new StaticFileOptions(new SharedOptions
            {
                FileProvider = provider,
            });
            configuration.Options.DefaultPage = "/index.html";
        });

        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<RestAPI>>();
                string endpointName = "";
                var requestDelegate = context.GetEndpoint()?.RequestDelegate;
                if (requestDelegate?.Method != null)
                {
                    endpointName = (requestDelegate.Method.DeclaringType?.Name ?? "<anonymous>") + "."
                        + requestDelegate.Method.Name;
                }
                logger.LogWarning($"Endpoint {context.GetEndpoint()?.DisplayName} encountered an exception: {ex}");
                await Results.InternalServerError("Request failed due to an exception: " + ex.Message).ExecuteAsync(context);
            }
        });
        app.MapFilesEndpoints();
        app.MapGithubEndpoints();
        app.MapDriverEndpoints();
        app.MapMapperEndpoints();
        app.MapMapperServiceEndpoints();

        app.MapGet("/favicon.png", () => Results.File(ApiHelper.EmbededResources.Favicon, contentType: "image/png"));
        app.MapGet("/dist/gameHookMapperClient.js", () => Results.File(ApiHelper.EmbededResources.ClientScript, contentType: "application/javascript"));
        app.MapHub<UpdateHub>("/updates");
    }
}