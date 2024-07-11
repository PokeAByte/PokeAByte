using GameHook.Application;
using GameHook.Domain.Interfaces;
using GameHook.Domain.Models;
using GameHook.Infrastructure.Drivers;
using GameHook.Infrastructure.Drivers.Bizhawk;
using GameHook.Infrastructure.Github;
using GameHook.Mappers;
using MudBlazor.Services;
using PokeAByte.Web.ClientNotifiers;
using PokeAByte.Web.Services;

namespace PokeAByte.Web;

public static class Startup
{
    public static void ConfigureServices(this IServiceCollection services)
    {
                    // Add services to the container.
            services.AddRazorComponents()
                .AddInteractiveServerComponents();
            services.AddMudServices();
            
            //Inherited services from GameHook
            services.AddHttpClient();
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
            services.AddSingleton<IGameHookInstance, GameHookInstance>();
            services.AddSingleton<ScriptConsole>();
            services.AddSingleton<IBizhawkMemoryMapDriver, BizhawkMemoryMapDriver>();
            services.AddSingleton<IRetroArchUdpPollingDriver, RetroArchUdpPollingDriver>();
            services.AddSingleton<IStaticMemoryDriver, StaticMemoryDriver>();
            services.AddSingleton<IClientNotifier, WebSocketClientNotifier>();
            
            services.AddSingleton<PropertyUpdateService>();
            //PokeAByte Services
            services.AddSingleton<MapperClientService>(x =>
            {
                var mapperFs = x.GetRequiredService<IMapperFilesystemProvider>();
                var logger = x.GetRequiredService<ILogger<MapperClientService>>();
                var clientNotif = x.GetRequiredService<IClientNotifier>();
                var propUpdate = x.GetRequiredService<PropertyUpdateService>();
                
                return new MapperClientService(mapperFs, logger, CreateClient(x), clientNotif, propUpdate);
            });
            services.AddScoped<NavigationService>();
    }
    private static MapperClient CreateClient(IServiceProvider services)
    {
        /*    ILogger<MapperClient> logger,
                IGameHookInstance instance,
                AppSettings appSettings,
                IBizhawkMemoryMapDriver bizhawkMemoryMapDriver,
                IRetroArchUdpPollingDriver retroArchUdpPollingDriver,
                IStaticMemoryDriver staticMemoryDriver*/
        var logger = services.GetRequiredService<ILogger<MapperClient>>();
        var instance = services.GetRequiredService<IGameHookInstance>();
        var appSettings = services.GetRequiredService<AppSettings>();
        var bizhawk = services.GetRequiredService<IBizhawkMemoryMapDriver>();
        var retro = services.GetRequiredService<IRetroArchUdpPollingDriver>();
        var staticMem = services.GetRequiredService<IStaticMemoryDriver>();
        return new MapperClient(logger, instance, appSettings, bizhawk, retro, staticMem);
    }

    public static void ConfigureApp(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
    }
}