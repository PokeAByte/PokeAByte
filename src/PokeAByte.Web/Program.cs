using System.Diagnostics;
using System.Reflection;
using PokeAByte.Domain.Models;
using Serilog;
using Serilog.Events;

namespace PokeAByte.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
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
            builder.Services.ConfigureServices();

            var app = builder.Build();

            LogVersion(app);
            app.ConfigureApp();
            var runTask = app.RunAsync();
            Process.Start(new ProcessStartInfo("http://localhost:8085/ui/mappers") { UseShellExecute = true });
            await runTask;
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