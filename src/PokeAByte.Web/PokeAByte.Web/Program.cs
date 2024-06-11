using System.Text.Json.Serialization;
using GameHook.Application;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Infrastructure;
using GameHook.Infrastructure.Drivers;
using GameHook.Infrastructure.Drivers.Bizhawk;
using GameHook.Infrastructure.Github;
using GameHook.Mappers;
using PokeAByte.Web.Client.Pages;
using PokeAByte.Web.Components;
using MudBlazor.Services;
using Serilog;

namespace PokeAByte.Web;

public class Program
{
    public static void Main(string[] args)
    {        
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("SERILOG_LOG_FILE_PATH", BuildEnvironment.LogFilePath);
        try
        {
            var builder = WebApplication.CreateBuilder(args);
        
            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateBootstrapLogger();

            Log.Information("Starting PokeAByte!");
            // Add services to the container.
            ConfigureServices(builder.Services, builder.Configuration);
            
            var app = builder.Build();

            var startup = app.Services.GetRequiredService<Startup>();
            startup.Configure(app);
            
            app.Run();
        }
        catch (Exception e)
        {
            Log.Error(e, "PokeAByte has exited due to an exception!");
        }
    }
    private static void ConfigureServices(IServiceCollection serviceCollection, ConfigurationManager configuration)
    {        
        //Add Blazor and MudBlazor
        serviceCollection.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();
        serviceCollection.AddMudServices();
        
        //Add Serilog
        serviceCollection.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext());
        
        //Add startup service
        serviceCollection.AddScoped<Startup>();
        
        //Add HTTP
        serviceCollection.AddHttpClient();
        serviceCollection.AddCors();

        //Add API controllers
        serviceCollection.AddControllers()
            .AddApplicationPart(typeof(Program).Assembly)
            .AddControllersAsServices()
            .AddJsonOptions(x => 
                x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
        
        //Add SignalR for API
        serviceCollection.AddSignalR();
        
        // Register application classes.
        serviceCollection.AddSingleton<AppSettings>();
        serviceCollection.AddSingleton<IGithubApiSettings>(x =>
        {
            var logger = x.GetRequiredService<ILogger<GithubApiSettings>>();
            var config = x.GetRequiredService<IConfiguration>();
            var token = config["GITHUB_TOKEN"];
            return GithubApiSettings.Load(logger, token);
        });
        serviceCollection.AddSingleton<IMapperFilesystemProvider, MapperFilesystemProvider>();
        serviceCollection.AddSingleton<IMapperUpdateManager, MapperUpdateManager>();
        serviceCollection.AddSingleton(x =>
        {
            var logger = x.GetRequiredService<ILogger<MapperUpdaterSettings>>();
            return MapperUpdaterSettings.Load(logger);
        });
        serviceCollection.AddSingleton<IMapperArchiveManager, MapperArchiveManager>(x =>
        {
            var logger = x.GetRequiredService<ILogger<MapperArchiveManager>>();
            var mapperArchiveManager = new MapperArchiveManager(logger);
            mapperArchiveManager.GenerateArchivedList();
            return mapperArchiveManager;
        });
        serviceCollection.AddSingleton<IGithubRestApi, GithubRestApi>();
        serviceCollection.AddSingleton<GameHookInstance>();
        serviceCollection.AddSingleton<ScriptConsole>();
        serviceCollection.AddSingleton<IBizhawkMemoryMapDriver, BizhawkMemoryMapDriver>();
        serviceCollection.AddSingleton<IRetroArchUdpPollingDriver, RetroArchUdpPollingDriver>();
        serviceCollection.AddSingleton<IStaticMemoryDriver, StaticMemoryDriver>();
        //serviceCollection.AddSingleton<IClientNotifier, WebSocketClientNotifier>();
    }
}