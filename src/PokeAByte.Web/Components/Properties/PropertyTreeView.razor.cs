using GameHook.Domain.Models.Properties;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.Properties;

public partial class PropertyTreeView : ComponentBase
{
    private MudTreeView<MapperPropertyTreeModel> _treeView;
    [Inject] public MapperConnectionService MapperConnectionService { get; set; }
    private HashSet<MapperPropertyTreeModel> PropertyItems { get; set; } = [];
    private MapperPropertyTreeModel? SelectedItem { get; set; }
    private string MapperName { get; set; } = "";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var metaResult = MapperConnectionService.GetMetaData();
        if (metaResult.IsSuccess)
            MapperName = $"{metaResult.ResultValue.GameName}";
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (!firstRender) return;
        var propsResult = MapperConnectionService.GetPropertiesHashSet();
        if (!propsResult.IsSuccess) return;
        PropertyItems = propsResult.ResultValue;
        foreach (var prop in PropertyItems)
        {
            MapperPropertyTreeModel.UpdateDisplayedChildren(prop);
        }
        StateHasChanged();
    }
    private string SetIcon(MapperPropertyTreeModel model)
    {
        return model.HasChildren
            ? (model.IsExpanded ? Icons.Material.Filled.FolderOpen : Icons.Material.Filled.Folder)
            : "";
    }
}