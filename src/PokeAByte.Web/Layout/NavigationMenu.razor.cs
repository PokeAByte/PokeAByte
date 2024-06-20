using Microsoft.AspNetCore.Components;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Layout;

public partial class NavigationMenu : ComponentBase, IDisposable
{
    [Inject] public NavigationService? NavService { get; set; }
    [Inject] public MapperConnectionService? ConnectionService { get; set; }
    
    private string _borderUnderlineClass = " border-b-4 border-solid mud-border-primary";
    private string _buttonBorder = "py-4 px-8 rounded-0";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (NavService is null)
            throw new InvalidOperationException("Navigation Service is null.");
        if (ConnectionService is null)
            throw new InvalidOperationException("Connection Service is null.");
        NavService.OnNavigation += StateHasChanged;
    }

    public void Dispose()
    {
        if(NavService is not null)
            NavService.OnNavigation -= StateHasChanged;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            NavService?.InitializeNavService();
            StateHasChanged();
        }
        base.OnAfterRender(firstRender);
    }
    
    private void NavButtonClickHandler(NavigationService.Pages page)
    {
        try
        {
            NavService?.Navigate(page);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}