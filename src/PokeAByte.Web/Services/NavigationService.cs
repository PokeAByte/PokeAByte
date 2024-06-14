using Microsoft.AspNetCore.Components;

namespace PokeAByte.Web.Services;

public class NavigationService
{
    private readonly NavigationManager _navigationManager;
    
    public NavigationService(NavigationManager navMan)
    {
        _navigationManager = navMan;
        CurrentPage = Pages.MapperManager;
    }
    public Pages CurrentPage { get; private set; }
    public string GetCurrentPageRoute() => GetPageRoute(CurrentPage);
    public static string GetPageRoute(Pages page) => page switch
    {
        Pages.Home => AppRoutes.Home,
        Pages.MapperConnectionStatus => AppRoutes.MapperConnectionStatus,
        Pages.MapperManager => AppRoutes.MapperManager,
        Pages.DataProperties => AppRoutes.DataProperties,
        Pages.AppSettings => AppRoutes.AppSettings,
        Pages.Error => AppRoutes.Error,
        _ => AppRoutes.Error
    };

    public void Navigate(Pages navTo)
    {
        CurrentPage = navTo;
        _navigationManager.NavigateTo(GetCurrentPageRoute());
    }
    public enum Pages
    {
        Home,
        MapperConnectionStatus,
        MapperManager,
        DataProperties,
        AppSettings,
        Error = 99
    }
}

public static class AppRoutes
{
    public const string Home = "/Home";
    public const string MapperConnectionStatus = "/ConnectionStatus";
    public const string MapperManager = "/MapperManager";
    public const string DataProperties = "/DataProperties";
    public const string AppSettings = "/AppSettings";
    public const string Error = "/Error";
}