using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.Properties;

public partial class PropertyExpansionPanel : ComponentBase, IDisposable
{
    [Inject] public MapperClientService MapperClientService { get; set; }
    //[Parameter] public MapperPropertyTreeModel Context { get; set; }
    [Parameter] public PropertyTreeModel Context { get; set; }
    private EditPropertyModel? _editContext;
    [Inject] public PropertyUpdateService PropertyUpdateService { get; set; }
    [Inject] public IJSRuntime JSRuntime { get; set; }
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            /*var helper = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("propertyExpansionClickHandler", helper);*/
        }
    }

    private async void HandlePropertyUpdate(object? sender, EventArgs e)
    {
        if (_editContext is null) return;
        MapperClientService.UpdateEditPropertyModel(_editContext);
        await InvokeAsync(StateHasChanged);
    }
    private Color SetIconColor =>
        Context.IsPropertyExpanded ? Color.Info : Color.Default;
    private string DisplayContent => Context.IsPropertyExpanded ? "display:block;" : "display:none;";

    private string PropertyHeight =>
        Context.IsPropertyExpanded ? "100%" : "35px";

    public void Dispose()
    {
        if (_editContext is null) return;
        PropertyUpdateService.EventHandlers.Remove(_editContext.Path);
    }

    public void OnClickExpand()
    {
        Context.IsPropertyExpanded = !Context.IsPropertyExpanded;
        StateHasChanged();
    }
    [JSInvokable]
    public void OnClickHandler(string id, string tag)
    {
        /*//propertyValueEditor
        if(tag is "TD" or "INPUT" || 
           (tag == "svg" && id != "expansionBall") ||
           (tag == "DIV" && id != "mudGrid"
                         && id != "expansionPanel" 
                         && id != "mudItemIcon"
                         && id != "mudText"
                         && id != "mudItem2"))
            return;
        Context.IsPropertyExpanded = !Context.IsPropertyExpanded;*/
    }
}