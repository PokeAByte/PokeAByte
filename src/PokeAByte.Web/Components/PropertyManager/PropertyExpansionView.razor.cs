using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;
using PokeAByte.Web.Services.Notifiers;

namespace PokeAByte.Web.Components.PropertyManager;

public partial class PropertyExpansionView : ComponentBase, IDisposable
{
    [Inject] public MapperClientService MapperClientService { get; set; }
    //private EditPropertyModel _editContext = new();
    
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Inject] public required IJSRuntime JSRuntime { get; set; }
    [Inject] public required PropertyUpdateService PropertyUpdateService { get; set; }
    [Parameter] public required PropertyTreePresenter Context { get; set; }
    [Parameter] public required PropertyTreeView Parent { get; set; }
    public Color IconColor =>
        Context.Value!.IsPropertyExpanded ? Color.Info : Color.Default;
    private string Width => $"width:{_textWidth}px;";
    private string PropertyHeight =>
        Context.Value?.IsPropertyExpanded is true ? "100%" : "25px";

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

    protected override void OnAfterRender(bool firstRender)
    {        

    }

    private async void HandlePropertyUpdate(object? sender, EventArgs e)
    {
        if (Context.Value?.PropertyModel is null) return;
        MapperClientService.UpdateEditPropertyModel(Context.Value.PropertyModel);
        //await InvokeAsync(StateHasChanged);
        await InvokeAsync(StateHasChanged);
        await InvokeAsync(Parent.RefreshParent);
        
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
}