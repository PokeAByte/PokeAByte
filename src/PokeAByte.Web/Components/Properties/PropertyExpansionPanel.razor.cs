using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.Properties;

public partial class PropertyExpansionPanel : ComponentBase, IDisposable
{
    [Inject] public MapperClientService MapperClientService { get; set; }
    [Parameter] public MapperPropertyTreeModel Context { get; set; }
    private EditPropertyModel? _editContext;
    [Inject] public PropertyUpdateService PropertyUpdateService { get; set; }
    [Parameter] public int TextWidth { get; set; }
    private string Width => $"width:{TextWidth}px;";

    protected override void OnInitialized()
    {
        if (Context.Property is not null)
        {
            _editContext = EditPropertyModel.FromPropertyModel(Context.Property);
            PropertyUpdateService.EventHandlers.TryAdd(_editContext.Path, HandlePropertyUpdate);
        }
        base.OnInitialized();
    }
    private async void HandlePropertyUpdate(object? sender, EventArgs e)
    {
        if (_editContext is null) return;
        MapperClientService.UpdateEditPropertyModel(_editContext);
        await InvokeAsync(StateHasChanged);
    }
    private Color SetIconColor =>
        Context.IsPropertyExpanded ? Color.Secondary : Color.Info;
    private string DisplayContent => Context.IsPropertyExpanded ? "display:block;" : "display:none;";

    public void Dispose()
    {
        if (_editContext is null) return;
        PropertyUpdateService.EventHandlers.Remove(_editContext.Path);
    }
}