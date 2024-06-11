using GameHook.Application;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Infrastructure;
using GameHook.Infrastructure.Drivers;
using MudBlazor.Services;
using PokeAByte.Web.Client.Pages;
using PokeAByte.Web.Components;
using PokeAByte.Web.Hubs;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace PokeAByte.Web;

public class Startup
{
    private readonly ILogger<Startup> _logger;
    private readonly IMapperUpdateManager _mapperUpdateManager;
    private readonly AppSettings _appSettings;
    
    public Startup(ILogger<Startup> logger,
        IMapperUpdateManager mapperUpdateManager, 
        AppSettings appSettings)
    {
        _logger = logger;
        _mapperUpdateManager = mapperUpdateManager;
        _appSettings = appSettings;
    }
    public void Configure(WebApplication app)
    {
        _logger.LogInformation("PokeAByte has started successfully, running configuration routine.");
        
        Directory.CreateDirectory(BuildEnvironment.ConfigurationDirectory);

        _logger.LogInformation("Checking for mapper updates.");
        var hasUpdates = _mapperUpdateManager.CheckForUpdates().GetAwaiter().GetResult();
        if (hasUpdates)
            _logger.LogInformation("Found new mapper updates!");
        
        app.UseCors(x =>
        {
            x.SetIsOriginAllowed(_ => true);
            x.AllowAnyMethod();
            x.AllowAnyHeader();
            x.AllowCredentials();
        });
        
        if (_appSettings.LOG_HTTP_TRAFFIC)
        {
            app.UseSerilogRequestLogging();
        }
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Counter).Assembly);

        app.MapControllers();

        app.MapHub<UpdateHub>("/updates");
        _logger.LogInformation("PokeAByte configuration has completed successfully!");
        _logger.LogInformation($"UI is accessible at {string.Join(", ", _appSettings.Urls)}");
    }
}