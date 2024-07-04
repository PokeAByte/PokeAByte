using GameHook.Domain.Models.Properties;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Pages;

public partial class DataProperties
{
    [CascadingParameter] public string? PageTitle { get; set; }
    [Inject] public MapperClientService ClientService { get; set; }
    private HashSet<MapperPropertyTreeModel> PropertyItems { get; set; } = [];
    private string MapperName { get; set; } = "";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var metaResult = ClientService.GetMetaData();
        if (metaResult.IsSuccess)
            MapperName = $"{metaResult.ResultValue?.GameName}";
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (!firstRender) return;
        var propsResult = ClientService.GetPropertiesHashSet();
        if (!propsResult.IsSuccess || propsResult.ResultValue is null) return;
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