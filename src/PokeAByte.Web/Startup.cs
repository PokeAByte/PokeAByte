using System.Reflection;
using System.Text.Json.Serialization;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.FileProviders;
using PokeAByte.Application;
using PokeAByte.Application.Mappers;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Infrastructure.Drivers;
using PokeAByte.Infrastructure.Drivers.Bizhawk;
using PokeAByte.Infrastructure.Drivers.UdpPolling;
using PokeAByte.Infrastructure.Github;
using PokeAByte.Web.ClientNotifiers;
using PokeAByte.Web.Hubs;
using PokeAByte.Web.Services.Drivers;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web;

public static class Startup
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        //Inherited services from GameHook
        services.AddHttpClient();
        services.AddCors();
        // Add Web API
        services
            .AddControllers()
            .AddApplicationPart(typeof(Program).Assembly)
            .AddControllersAsServices()
            .AddProblemDetailsConventions()
            .AddJsonOptions(x => x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

        services.AddSignalR();

        services.AddProblemDetails((options) =>
        {
            options.ShouldLogUnhandledException = (ctx, e, d) => true;

            options.IncludeExceptionDetails = (ctx, ex) =>
            {
                return BuildEnvironment.IsDebug;
            };
        });
        services.AddSingleton<AppSettings>();
        services.AddSingleton<IMapperFilesystemProvider, MapperFilesystemProvider>();
        services.AddSingleton<IGithubApiSettings>(x =>
        {
            var logger = x.GetRequiredService<ILogger<GithubApiSettings>>();
            var config = x.GetRequiredService<IConfiguration>();
            var token = config["GITHUB_TOKEN"];
            return GithubApiSettings.Load(logger, token);
        });
        services.AddSingleton<IMapperUpdateManager, MapperUpdateManager>();
        services.AddSingleton(x =>
        {
            var logger = x.GetRequiredService<ILogger<MapperUpdaterSettings>>();
            return MapperUpdaterSettings.Load(logger);
        });
        services.AddSingleton<IMapperArchiveManager, MapperArchiveManager>(x =>
        {
            var logger = x.GetRequiredService<ILogger<MapperArchiveManager>>();
            var mapperArchiveManager = new MapperArchiveManager(logger);
            mapperArchiveManager.GenerateArchivedList();
            return mapperArchiveManager;
        });
        services.AddSingleton<IGithubRestApi, GithubRestApi>();        
        services.AddSingleton<ScriptConsole>();
        services.AddSingleton<IBizhawkMemoryMapDriver, BizhawkMemoryMapDriver>();
        services.AddSingleton<IRetroArchUdpPollingDriver, RetroArchUdpDriver>();
        services.AddSingleton<IStaticMemoryDriver, StaticMemoryDriver>();
        services.AddSingleton<DriverService>();
        services.AddSingleton<IClientNotifier, WebSocketClientNotifier>();
        services.AddSingleton<MapperClientService>();
        services.AddSingleton<IPokeAByteInstance, PokeAByteInstance>();
    }

    public static void ConfigureApp(this WebApplication app)
    {
        Directory.CreateDirectory(BuildEnvironment.ConfigurationDirectory);
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseCors(x =>
        {
            x.SetIsOriginAllowed(x => true);
            x.AllowAnyMethod();
            x.AllowAnyHeader();
            x.AllowCredentials();
        });
        app.UseProblemDetails();
        app.UseRouting();
        app.UseStaticFiles();

        var provider = new ManifestEmbeddedFileProvider(Assembly.GetEntryAssembly()!);
        app.UseSpaStaticFiles(new StaticFileOptions
        {
            FileProvider = provider,
            RequestPath = "",
        });
        app.UseSpa(configuration =>
        {
            configuration.Options.DefaultPageStaticFileOptions = new StaticFileOptions(new SharedOptions
            {
                FileProvider = provider,
            });
            configuration.Options.DefaultPage = "/index.html";
        });


        if (!BuildEnvironment.IsDebug)
        {
            app.MapGet("/favicon.ico", () => Results.File(ApiHelper.EmbededResources.favicon_ico, contentType: "image/x-icon"));
            app.MapGet("/site.css", () => Results.File(ApiHelper.EmbededResources.site_css, contentType: "text/css"));
            app.MapGet("/dist/gameHookMapperClient.js", () => Results.File(ApiHelper.EmbededResources.dist_gameHookMapperClient_js, contentType: "application/javascript"));
        }

        app.MapControllers();
        app.MapHub<UpdateHub>("/updates");
    }
}