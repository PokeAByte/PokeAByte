using System.Diagnostics;
using PokeAByte.Domain.Models;
using Serilog;
using Serilog.Events;

namespace PokeAByte.Web;

public class Program
{
    public static void Main(string[] args)
    {          
        Environment.SetEnvironmentVariable("SERILOG_LOG_FILE_PATH", BuildEnvironment.LogFilePath);      
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(BuildEnvironment.LogFilePath)
            .CreateBootstrapLogger();
        try
        {
            if (!File.Exists(BuildEnvironment.UserSettingsJson))
                File.Create(BuildEnvironment.UserSettingsJson);
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

            app.ConfigureApp();
            Process.Start(new ProcessStartInfo("http://localhost:8085") { UseShellExecute = true});
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


}