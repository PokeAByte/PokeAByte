using System.Diagnostics;
using System.Reflection;
using PokeAByte.Domain.Models;
using PokeAByte.Web.Logger;

namespace PokeAByte.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        ILogger? logger = null;
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddPokeAByteLogger(o =>
            {
                o.LogLevel = LogLevel.Information;
                o.LogFile = BuildEnvironment.LogFilePath;
                o.AddOverride("Microsoft.Hosting.Lifetime", LogLevel.Information);
                o.AddOverride("Microsoft", LogLevel.Warning);
                o.AddOverride("System.Net.Http.HttpClient", LogLevel.Warning);
                o.FileSizeLimit = 8_000_000; // ~8 megabytes worth of logs should be enough.
            });
            builder.Services.ConfigureServices();

            var app = builder.Build();
            logger = app.Services.GetRequiredService<ILogger<Program>>();

            LogVersion(app);
            app.ConfigureApp();
            var runTask = app.RunAsync();
            Process.Start(new ProcessStartInfo("http://localhost:8085/ui/mappers") { UseShellExecute = true });
            await runTask;
        }
        catch (Exception ex)
        {
            logger?.LogCritical(ex, "Poke-A-Byte startup failed!");
        }
    }

    private static void LogVersion(WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        var version = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyFileVersionAttribute>()
            ?.Version;
        logger.LogInformation($"Poke-A-Byte version: {version}");
        var informationalVersion = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        if (informationalVersion != null)
        {
            var commit = informationalVersion.Contains("+")
                ? informationalVersion.Split("+")[1]
                : "<unknown>";
            logger.LogInformation($"Poke-A-Byte commit: {commit}");
        }
    }
}