using System.Text.Json.Serialization;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using MudBlazor;
using MudBlazor.Services;
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
using PokeAByte.Web.Services;
using PokeAByte.Web.Services.Drivers;
using PokeAByte.Web.Services.Mapper;
using PokeAByte.Web.Services.Navigation;
using PokeAByte.Web.Services.Notifiers;
using PokeAByte.Web.Services.Properties;

namespace PokeAByte.Web;

public static class Startup
{
    public static void ConfigureServices(this IServiceCollection services)
    {
            // Add services to the container.
            services.AddRazorComponents()
                .AddInteractiveServerComponents();
            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 2500;
                config.SnackbarConfiguration.HideTransitionDuration = 300;
                config.SnackbarConfiguration.ShowTransitionDuration = 300;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });
            
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
            services.AddSingleton<PokeAByteInstance>();
            services.AddSingleton<ScriptConsole>();
            services.AddSingleton<IBizhawkMemoryMapDriver, BizhawkMemoryMapDriver>();
            services.AddSingleton<IRetroArchUdpPollingDriver, RetroArchUdpPollingDriver>();
            services.AddSingleton<IStaticMemoryDriver, StaticMemoryDriver>();
            services.AddSingleton<DriverService>();
            services.AddSingleton<IClientNotifier, WebSocketClientNotifier>();
            
            services.AddSingleton<PropertyUpdateService>();
            services.AddSingleton<MapperSettingsService>();
            //PokeAByte Services
            services.AddSingleton<MapperClientService>(x =>
            {
                var mapperFs = x.GetRequiredService<IMapperFilesystemProvider>();
                var logger = x.GetRequiredService<ILogger<MapperClientService>>();
                var clientNotif = x.GetRequiredService<IClientNotifier>();
                var propUpdate = x.GetRequiredService<PropertyUpdateService>();
                var driverService = x.GetRequiredService<DriverService>();
                var mapperSettings = x.GetRequiredService<MapperSettingsService>();
                return new MapperClientService(mapperFs, 
                    logger,
                    CreateClient(x),
                    clientNotif, 
                    propUpdate, 
                    driverService);
            });
            services.AddScoped<MapperManagerService>();
            services.AddScoped<NavigationService>();
            services.AddScoped<ChangeNotificationService>();
            services.AddScoped<PropertyService>();
            //For some reason, the Driver controller requires special DI despite not needing it 
            //in the original implementation? Just add them to the DI
            services.AddSingleton<IPokeAByteInstance, PokeAByteInstance>();
            services.AddSingleton<IPokeAByteDriver, StaticMemoryDriver>();
    }
    private static MapperClient CreateClient(IServiceProvider services)
    {
        /*    ILogger<MapperClient> logger,
                IPokeAByteInstance instance,
                AppSettings appSettings,
                IBizhawkMemoryMapDriver bizhawkMemoryMapDriver,
                IRetroArchUdpPollingDriver retroArchUdpPollingDriver,
                IStaticMemoryDriver staticMemoryDriver*/
        var logger = services.GetRequiredService<ILogger<MapperClient>>();
        var instance = services.GetRequiredService<PokeAByteInstance>();
        var appSettings = services.GetRequiredService<AppSettings>();
        var bizhawk = services.GetRequiredService<IBizhawkMemoryMapDriver>();
        var retro = services.GetRequiredService<IRetroArchUdpPollingDriver>();
        var staticMem = services.GetRequiredService<IStaticMemoryDriver>();
        var mapperSettings = services.GetRequiredService<MapperSettingsService>();
        return new MapperClient(logger, instance, appSettings, bizhawk, retro, staticMem, mapperSettings);
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
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        if (BuildEnvironment.IsDebug)
        {
            //app.MapGet("/gh_index", () => Results.Redirect("index.html", false));
        }
        else
        {
            //app.MapGet("/gh_index", () => Results.File(ApiHelper.EmbededResources.index_html, contentType: "text/html"));
            app.MapGet("/favicon.ico", () => Results.File(ApiHelper.EmbededResources.favicon_ico, contentType: "image/x-icon"));
            app.MapGet("/site.css", () => Results.File(ApiHelper.EmbededResources.site_css, contentType: "text/css"));
            app.MapGet("/dist/gameHookMapperClient.js", () => Results.File(ApiHelper.EmbededResources.dist_gameHookMapperClient_js, contentType: "application/javascript"));
        }

        app.MapControllers();
        app.MapHub<UpdateHub>("/updates");
        app.Services.GetRequiredService<DriverService>();
    }
}