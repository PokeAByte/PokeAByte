using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using PokeAByte.Web.Models;

namespace PokeAByte.Web.Components.Properties;

public partial class PropertyExpansionPanel : ComponentBase
{
    [Parameter] public MapperPropertyTreeModel Context { get; set; }
    public EditPropertyModel EditContext { get; private set; }
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Context.Property is null) return;
        EditContext = EditPropertyModel.FromPropertyModel(Context.Property);
    }

    private Color SetIconColor =>
        Context.IsPropertyExpanded ? Color.Secondary : Color.Info;
    private string DisplayContent => Context.IsPropertyExpanded ? "display:block;" : "display:none;";
}