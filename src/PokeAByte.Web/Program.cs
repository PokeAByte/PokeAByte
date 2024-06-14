using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Domain.Models;
using GameHook.Mappers;
using Microsoft.AspNetCore.Components;
using MudBlazor.Services;
using PokeAByte.Web.Services;

namespace PokeAByte.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddMudServices();
        
        builder.Services.AddScoped<AppSettings>();
        builder.Services.AddScoped<IMapperFilesystemProvider, MapperFilesystemProvider>();
        builder.Services.AddScoped<MapperConnectionService>();
        builder.Services.AddScoped<NavigationService>();
        
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
}