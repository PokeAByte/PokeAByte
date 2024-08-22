using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;
using PokeAByte.Web.Services;
using PokeAByte.Web.Services.Mapper;
using PokeAByte.Web.Services.Navigation;
using PokeAByte.Web.Services.Notifiers;

namespace PokeAByte.Web.Layout;

public partial class NavigationMenu : ComponentBase, IDisposable, IBrowserViewportObserver
{
    [Inject] public NavigationService? NavService { get; set; }
    [Inject] public MapperClientService? ConnectionService { get; set; }
    [Inject] private ChangeNotificationService ChangeNotificationService { get; set; }
    [Inject] public required IBrowserViewportService BrowserViewportService { get; set; }
    private string? _breakpointStyle;

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
        ChangeNotificationService.OnChange += StateHasChanged;
        ConnectionService.OnMapperIsUnloaded += OnMapperIsUnloadedHandler;
    }

    private async void OnMapperIsUnloadedHandler()
    {
        NavService?.DisablePropertiesButton();
        await InvokeAsync(StateHasChanged);
        //NavService?.TogglePropertiesButton();
    }

    public void Dispose()
    {
        if(NavService is not null)
            NavService.OnNavigation -= StateHasChanged;
        ChangeNotificationService.OnChange -= StateHasChanged;
        if(ConnectionService is not null)
            ConnectionService.OnMapperIsUnloaded -= OnMapperIsUnloadedHandler;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            NavService?.InitializeNavService();
            await BrowserViewportService.SubscribeAsync(this, fireImmediately: true);
            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
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

    private void OnClickHomeButton()
    {
        NavService?.Navigate(ConnectionService.IsCurrentlyConnected ?
            NavigationService.Pages.Properties : NavigationService.Pages.MapperManager);
    }

    public async ValueTask DisposeAsync() => await BrowserViewportService.UnsubscribeAsync(this);
    
    Guid IBrowserViewportObserver.Id { get; } = Guid.NewGuid();

    ResizeOptions IBrowserViewportObserver.ResizeOptions { get; } = new()
    {
        ReportRate = 250,
        NotifyOnBreakpointOnly = true
    };
    public Task NotifyBrowserViewportChangeAsync(BrowserViewportEventArgs browserViewportEventArgs)
    {
        if (browserViewportEventArgs.Breakpoint == Breakpoint.Xs)
        {
            _breakpointStyle = "padding-top:0px; margin-bottom: 30px";
        }
        else if (browserViewportEventArgs.BrowserWindowSize.Width >= 600)
        {
            _breakpointStyle = "padding-top:24px; margin-bottom:0px";
        }
        return InvokeAsync(StateHasChanged);
    }

    private Color GetActiveColor(NavigationService.Pages page) =>
        NavService!.CurrentPage == page ? Color.Info : Color.Default;
    
}