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
            MapperName = $"{metaResult.ResultValue.GameName} - {metaResult.ResultValue.Id}";
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

    /*private static HashSet<MapperPropertyTreeModel> GetChildren(HashSet<MapperPropertyTreeModel> node)
    {
        return node
            .Select(MapperPropertyTreeModel.CreateFrom)
            .ToHashSet();
    }*/
    private string SetIcon(MapperPropertyTreeModel model)
    {
        return model.HasChildren
            ? (model.IsExpanded ? Icons.Material.Filled.FolderOpen : Icons.Material.Filled.Folder)
            : "";
        /*return model.HasChildren ?
            (model.IsExpanded ?
                Icons.Material.Filled.FolderOpen :
                Icons.Material.Filled.Folder) :
            Icons.Material.Outlined.CatchingPokemon;*/
    }
    
    private async Task<HashSet<MapperPropertyTreeModel>> LoadPropertyData(MapperPropertyTreeModel parent)
    {
        await Task.Delay(0);
        try
        {
            return parent.Children;
            /*MapperPropertyTreeModel.UpdateDisplayedChildren(parent.DisplayedChildren);
            return parent.DisplayedChildren;*/
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private Color SetIconColor(MapperPropertyTreeModel context) =>
        context.IsPropertyExpanded ? Color.Secondary : Color.Info;
}