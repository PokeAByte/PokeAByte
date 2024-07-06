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
using Serilog;
using Serilog.Events;

namespace PokeAByte.Web;

public class Program
{
    public static void Main(string[] args)
    {          
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("SERILOG_LOG_FILE_PATH", BuildEnvironment.LogFilePath);      
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(BuildEnvironment.LogFilePath)
            .CreateBootstrapLogger();
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((bc, sp, lc) => 
                lc.MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Serilog.AspNetCore.RequestLoggingMiddleware", LogEventLevel.Warning)
                    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                    .WriteTo.Console(outputTemplate: 
                        "{Timestamp:HH:mm} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}")
                    .WriteTo.File(path: BuildEnvironment.LogFilePath, 
                        fileSizeLimitBytes: 16000000,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}")
                    .Enrich.FromLogContext());
            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddMudServices();
            
            //Inherited services from GameHook
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<AppSettings>();
            builder.Services.AddSingleton<IMapperFilesystemProvider, MapperFilesystemProvider>();
            builder.Services.AddSingleton<IGithubApiSettings>(x =>
            {
                var logger = x.GetRequiredService<ILogger<GithubApiSettings>>();
                var config = x.GetRequiredService<IConfiguration>();
                var token = config["GITHUB_TOKEN"];
                return GithubApiSettings.Load(logger, token);
            });
            builder.Services.AddSingleton<IMapperUpdateManager, MapperUpdateManager>();
            builder.Services.AddSingleton(x =>
            {
                var logger = x.GetRequiredService<ILogger<MapperUpdaterSettings>>();
                return MapperUpdaterSettings.Load(logger);
            });
            builder.Services.AddSingleton<IMapperArchiveManager, MapperArchiveManager>(x =>
            {
                var logger = x.GetRequiredService<ILogger<MapperArchiveManager>>();
                var mapperArchiveManager = new MapperArchiveManager(logger);
                mapperArchiveManager.GenerateArchivedList();
                return mapperArchiveManager;
            });
            builder.Services.AddSingleton<IGithubRestApi, GithubRestApi>();
            builder.Services.AddSingleton<IGameHookInstance, GameHookInstance>();
            builder.Services.AddSingleton<ScriptConsole>();
            builder.Services.AddSingleton<IBizhawkMemoryMapDriver, BizhawkMemoryMapDriver>();
            builder.Services.AddSingleton<IRetroArchUdpPollingDriver, RetroArchUdpPollingDriver>();
            builder.Services.AddSingleton<IStaticMemoryDriver, StaticMemoryDriver>();
            builder.Services.AddSingleton<IClientNotifier, WebSocketClientNotifier>();
            
            builder.Services.AddSingleton<PropertyUpdateService>();
            //PokeAByte Services
            builder.Services.AddSingleton<MapperClientService>(x =>
            {
                var mapperFs = x.GetRequiredService<IMapperFilesystemProvider>();
                var logger = x.GetRequiredService<ILogger<MapperClientService>>();
                var clientNotif = x.GetRequiredService<IClientNotifier>();
                var propUpdate = x.GetRequiredService<PropertyUpdateService>();
                
                return new MapperClientService(mapperFs, logger, CreateClient(x), clientNotif, propUpdate);
            });
            builder.Services.AddScoped<NavigationService>();
            //builder.Services.AddSingleton<MapperClientService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
            
            app.Run();     
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Poke-A-Byte startup failed!");
        }
        finally
        {
            Log.CloseAndFlush();
        }
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
}