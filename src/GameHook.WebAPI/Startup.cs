using GameHook.Application;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Infrastructure;
using GameHook.Infrastructure.Drivers;
using GameHook.WebAPI.ClientNotifiers;
using GameHook.WebAPI.Hubs;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Serialization;

namespace GameHook.WebAPI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddCors();

            // Add Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(x =>
            {
                x.DocumentFilter<DefaultSwashbuckleFilter>();

                x.EnableAnnotations();

                // Use method name as operationId
                x.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });

                x.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "GameHook API",
                    Contact = new OpenApiContact
                    {
                        Name = "GameHook",
                        Url = new Uri("https://gamehook.io/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "GNU Affero General Public License v3.0",
                        Url = new Uri("https://github.com/gamehook-io/gamehook/blob/main/LICENSE.txt")
                    }
                });
            });

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

            // Register application classes.
            services.AddSingleton<AppSettings>();
            services.AddSingleton<GameHookInstance>();
            services.AddSingleton<ScriptConsole>();
            services.AddSingleton<IMapperFilesystemProvider, MapperFilesystemProvider>();
            services.AddSingleton<IMapperUpdateManager, MapperUpdateManager>();
            services.AddSingleton<IBizhawkMemoryMapDriver, BizhawkMemoryMapDriver>();
            services.AddSingleton<IRetroArchUdpPollingDriver, RetroArchUdpPollingDriver>();
            services.AddSingleton<IStaticMemoryDriver, StaticMemoryDriver>();
            services.AddSingleton<IClientNotifier, WebSocketClientNotifier>();
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger, AppSettings appSettings, IMapperUpdateManager mapperUpdateManager)
        {
            if (BuildEnvironment.IsTestingBuild)
            {
                logger.LogWarning("WARNING: This is a debug build for testing!");
                logger.LogWarning("Please upgrade to the latest stable release.");
            }

            Directory.CreateDirectory(BuildEnvironment.ConfigurationDirectory);

            logger.LogInformation($"GameHook version: {BuildEnvironment.AssemblyVersion}.");
            logger.LogInformation($"Mapper version: {appSettings.MAPPER_VERSION}.");

            mapperUpdateManager.CheckForUpdates().GetAwaiter().GetResult();

            app.UseCors(x =>
            {
                x.SetIsOriginAllowed(x => true);
                x.AllowAnyMethod();
                x.AllowAnyHeader();
                x.AllowCredentials();
            });

            // Use Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            if (appSettings.LOG_HTTP_TRAFFIC == true)
            {
                app.UseSerilogRequestLogging();
            }

            app.UseProblemDetails();
            app.UseRouting();

            if (BuildEnvironment.IsDebug)
            {
                app.UseStaticFiles();
            }

            app.UseEndpoints(x =>
            {
                if (BuildEnvironment.IsDebug)
                {
                    x.MapGet("/", () => Results.Redirect("index.html", false));
                }
                else
                {
                    x.MapGet("/", () => Results.File(EmbededResources.index_html, contentType: "text/html"));
                    x.MapGet("/favicon.ico", () => Results.File(EmbededResources.favicon_ico, contentType: "image/x-icon"));
                    x.MapGet("/site.css", () => Results.File(EmbededResources.site_css, contentType: "text/css"));
                    x.MapGet("/dist/gameHookMapperClient.js", () => Results.File(EmbededResources.dist_gameHookMapperClient_js, contentType: "application/javascript"));
                }

                x.MapControllers();

                x.MapHub<UpdateHub>("/updates");
            });

            logger.LogInformation("GameHook startup completed.");
            logger.LogInformation($"UI is accessible at {string.Join(", ", appSettings.Urls)}");
        }
    }
}