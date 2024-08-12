using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;
using PokeAByte.Web.Services.Notifiers;

namespace PokeAByte.Web.Components.PropertyManager;

public partial class PropertyExpansionView : ComponentBase, IDisposable, IBrowserViewportObserver
{
    [Inject] public MapperClientService MapperClientService { get; set; }
    //private EditPropertyModel _editContext = new();
    
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Inject] public required IJSRuntime JSRuntime { get; set; }
    [Inject] public required PropertyUpdateService PropertyUpdateService { get; set; }
    [Inject] public required IBrowserViewportService BrowserViewportService { get; set; }
    [Parameter] public required PropertyTreePresenter Context { get; set; }
    [Parameter] public required PropertyTreeView Parent { get; set; }
    public Color IconColor =>
        Context.Value!.IsPropertyExpanded ? Color.Info : Color.Default;
    private string Width => $"width:{_textWidth}px;";
    private string _propertyHeight = "";
    private readonly int _browserWidthBreakpoint = 750;
    private readonly string _propertyWidth = "100%";
    private string DisplayContent => 
        Context.Value?.IsPropertyExpanded is true ? "display:block;" : "display:none;";

    private int _textWidth = 125;
    protected override void OnInitialized()
    {
        if(Context.Value is null)
            throw new InvalidOperationException("Context value is null.");
        if (Context.Value!.PropertyModel is not null)
        {
            //_editContext = EditPropertyModel.FromPropertyModel(Context.Value!.PropertyModel);
            var foundEventHandler = PropertyUpdateService
                .EventHandlers
                .FirstOrDefault(x => x.Key == Context.Value!.PropertyModel.Path);
            if (!string.IsNullOrWhiteSpace(foundEventHandler.Key))
            {
                PropertyUpdateService.EventHandlers.Remove(foundEventHandler.Key);
            }
            PropertyUpdateService.EventHandlers.TryAdd(Context.Value!.PropertyModel.Path, HandlePropertyUpdate);
        }
        if (Context.Parent is not null)
        {
            _textWidth = Context.Parent.GetMaxLength() * 10;
        }
        else
        {
            _textWidth = string.IsNullOrEmpty(Context.Text) ? 125 : Context.Text.Length * 10;
        }
        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {        
        if (firstRender)
        {
            await BrowserViewportService.SubscribeAsync(this, fireImmediately: true);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async void HandlePropertyUpdate(object? sender, EventArgs e)
    {
        if (Context.Value?.PropertyModel is null) return;
        //Console.WriteLine($"{Context.Value.PropertyModel.Path}");
        MapperClientService.UpdateEditPropertyModel(Context.Value.PropertyModel);
        //await InvokeAsync(StateHasChanged);
        await InvokeAsync(StateHasChanged);
        
        await Task.Delay(100);
        if(Context.Value?.PropertyModel?.Value is null) return;
        if (Context.Value.PropertyModel.ValueString != Context.Value.PropertyModel.Value?.ToString())
        {
            Context.Value.PropertyModel.ValueString = Context.Value.PropertyModel.Value!.ToString()!;
        }
        
        await Task.Delay(100);
        if (Context.Value.PropertyModel.ValueString != Context.Value.PropertyModel.Value?.ToString())
        {
            Context.Value.PropertyModel.ValueString = Context.Value.PropertyModel.Value!.ToString()!;
        }
    }
    private void OnClickExpand()
    {       
        Context.Value!.IsPropertyExpanded = !Context.Value.IsPropertyExpanded;
        if (Context.Value!.IsPropertyExpanded)
        {
            _propertyHeight = "100%";
        }
        else
        {
            _propertyHeight = _browserWidth >= _browserWidthBreakpoint ? _propertyWidth : "100%";
        }
        //StateHasChanged();
    }

    public void Dispose()
    {
        if(Context.Value?.PropertyModel?.Path is not null)
            PropertyUpdateService.EventHandlers.Remove(Context.Value.PropertyModel.Path);
    }

    private async Task CopyToClipboard(object? copy)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", copy);
            Snackbar.Add($"Copied {copy} to the clipboard!", 
                Severity.Info);
        }
        catch (Exception e)
        {
            var msg = "Failed to copy to clipboard!";
            Snackbar.Add(msg, Severity.Error);
        }
    }

    private int _browserWidth = 0;
    public Task NotifyBrowserViewportChangeAsync(BrowserViewportEventArgs browserViewportEventArgs)
    {
        _browserWidth = browserViewportEventArgs.BrowserWindowSize.Width;
        if (Context.Value?.IsPropertyExpanded is true)
        {
            _propertyHeight = "100%";
        }
        else if (browserViewportEventArgs.BrowserWindowSize.Width < _browserWidthBreakpoint)
        {
            _propertyHeight = "100%";
        }
        else if (browserViewportEventArgs.BrowserWindowSize.Width >= _browserWidthBreakpoint)
        {
            _propertyHeight = _propertyWidth;
        }
        
        return InvokeAsync(StateHasChanged);
    }
    public async ValueTask DisposeAsync() => await BrowserViewportService.UnsubscribeAsync(this);
    
    Guid IBrowserViewportObserver.Id { get; } = Guid.NewGuid();

    ResizeOptions IBrowserViewportObserver.ResizeOptions { get; } = new()
    {
        ReportRate = 250,
        NotifyOnBreakpointOnly = false
    };
}