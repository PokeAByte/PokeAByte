using Microsoft.AspNetCore.Components;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Services.Navigation;

public class NavigationService
{
    public event Action OnNavigation;
    
    private readonly List<ButtonModel> _navigationButtons = [];
    private readonly NavigationManager _navigationManager;
    public NavigationService(NavigationManager navMan,
        MapperClientService mapperClientService)
    {
        _navigationManager = navMan;
        //Todo: This is not always the case
        CurrentPage = GetPageFromUri(_navigationManager.Uri);
    }

    public void InitializeNavService()
    {
        _navigationButtons.Add(new ButtonModel
        {
            Page = Pages.MapperManager,
            IsDeactivated = false
        });
        _navigationButtons.Add(new ButtonModel
        {
            Page = Pages.Properties, 
            IsDeactivated = true
        });
        _navigationButtons.Add(new ButtonModel
        {
            Page = Pages.AppSettings,
            IsDeactivated = false
        });
        _navigationButtons.Add(new ButtonModel
        {
            Page = Pages.MapperConnectionStatus,
            IsDeactivated = false
        });
        var currentPage = _navigationButtons
            .FirstOrDefault(x => x.Page == CurrentPage);
        if (currentPage is not null)
        {
            currentPage.IsDeactivated = true;
        }
    }
    public bool? IsButtonDisabled(Pages page) =>
        _navigationButtons.FirstOrDefault(x => x.Page == page)?.IsDeactivated;
    public Pages CurrentPage { get; private set; }
    public string GetCurrentPageRoute() => GetPageRoute(CurrentPage);
    public static string GetPageRoute(Pages page) => page switch
    {
        Pages.Home => AppRoutes.Home,
        Pages.MapperConnectionStatus => AppRoutes.MapperConnectionStatus,
        Pages.MapperManager => AppRoutes.MapperManager,
        Pages.Properties => AppRoutes.Properties,
        Pages.AppSettings => AppRoutes.AppSettings,
        Pages.Error => AppRoutes.Error,
        _ => AppRoutes.Error
    };

    public static Pages GetPageFromUri(string uri)
    {
        var uriSplit = uri.Split('/');
        if (uriSplit.Length < 1)
            return Pages.Home;
        var pageString = $"/{uriSplit[^1]}";
        return pageString switch
        {
            AppRoutes.Home => Pages.Home,
            AppRoutes.MapperManager => Pages.MapperManager,
            AppRoutes.Properties => Pages.Properties,
            AppRoutes.AppSettings => Pages.AppSettings,
            AppRoutes.MapperConnectionStatus => Pages.MapperConnectionStatus,
            _ => Pages.Error
        };
    }
    public void NotifyStateChange() => OnNavigation?.Invoke();
    public void Navigate(Pages navTo)
    {
        if (CurrentPage == navTo)
            return;
        _navigationButtons
            .FirstOrDefault(x => x.Page == CurrentPage)?
            .SetDeactivated(false);
        CurrentPage = navTo;
        _navigationButtons
            .FirstOrDefault(x => x.Page == CurrentPage)?
            .SetDeactivated(true);
        _navigationManager.NavigateTo(GetCurrentPageRoute());
        NotifyStateChange();
    }
    public enum Pages
    {
        Home,
        MapperConnectionStatus,
        MapperManager,
        Properties,
        AppSettings,
        Error = 99,
    }

    public void TogglePropertiesButton()
    {
        var propButton = _navigationButtons
            .FirstOrDefault(x => x.Page == Pages.Properties);
        if (propButton is null) return;
        propButton.IsDeactivated = !propButton.IsDeactivated;
    }

    public void DisablePropertiesButton()
    {
        var propButton = _navigationButtons
            .FirstOrDefault(x => x.Page == Pages.Properties);
        if (propButton is null) return;
        propButton.IsDeactivated = true;   
    }
}

public static class AppRoutes
{
    public const string Home = "/Home";
    public const string MapperConnectionStatus = "/ConnectionStatus";
    public const string MapperManager = "/MapperManager";
    public const string Properties = "/Properties";
    public const string AppSettings = "/AppSettings";
    public const string Error = "/Error";
}