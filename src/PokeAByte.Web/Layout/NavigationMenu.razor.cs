using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Layout;

public partial class NavigationMenu : ComponentBase
{
    [Inject] public NavigationService? NavService { get; set; }
    [Inject] public MapperConnectionService? ConnectionService { get; set; }
    
    private MudBaseButton? _currentBtn;
    private MudBaseButton _mapperManager;
    private MudBaseButton _dataProperties;
    private MudBaseButton _appSettings;
    private MudBaseButton _connectionStatus;
    
    private string _borderUnderlineClass = " border-b-4 border-solid mud-border-primary";
    private string _buttonBorder = "py-4 px-8 rounded-0";
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (NavService is null)
            throw new InvalidOperationException("Navigation Service is null.");
        if (ConnectionService is null)
            throw new InvalidOperationException("Connection Service is null.");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _currentBtn = _mapperManager;
            _mapperManager.Disabled = true;
            StateHasChanged();
        }
        base.OnAfterRender(firstRender);
    }

    private void NavButtonClickHandler(NavigationService.Pages page, MudBaseButton caller)
    {
        if (_currentBtn is not null && caller == _currentBtn)
        {
            //Same page, just return
            return;
        }
        _currentBtn.Disabled = false;
        caller.Disabled = true;
        _currentBtn = caller;
        NavService?.Navigate(page);
    }
}