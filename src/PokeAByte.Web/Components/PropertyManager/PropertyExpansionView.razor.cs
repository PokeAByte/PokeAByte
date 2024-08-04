using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;
using PokeAByte.Web.Services.Notifiers;

namespace PokeAByte.Web.Components.PropertyManager;

public partial class PropertyExpansionView : ComponentBase, IDisposable
{
    [Inject] public MapperClientService MapperClientService { get; set; }
    private EditPropertyModel _editContext = new();
    [Inject] public required PropertyUpdateService PropertyUpdateService { get; set; }
    [Parameter] public required PropertyTreePresenter Context { get; set; }
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
            _editContext = EditPropertyModel.FromPropertyModel(Context.Value!.PropertyModel);
            PropertyUpdateService.EventHandlers.TryAdd(_editContext.Path, HandlePropertyUpdate);
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
    private async void HandlePropertyUpdate(object? sender, EventArgs e)
    {
        if (_editContext is null) return;
        MapperClientService.UpdateEditPropertyModel(_editContext);
        await InvokeAsync(StateHasChanged);
    }
    private void OnClickExpand()
    {       
        Context.Value!.IsPropertyExpanded = !Context.Value!.IsPropertyExpanded;
        
        //StateHasChanged();
    }

    public void Dispose()
    {
        PropertyUpdateService.EventHandlers.Remove(_editContext.Path);
    }
}