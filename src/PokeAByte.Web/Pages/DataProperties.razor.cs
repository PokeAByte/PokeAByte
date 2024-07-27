using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Pages;

public partial class DataProperties
{
    [CascadingParameter] public string? PageTitle { get; set; }
    [Inject] public MapperClientService ClientService { get; set; }
    [Inject] public NavigationService? NavService { get; set; }
    [Inject] private ChangeNotificationService ChangeNotificationService { get; set; }
    //private HashSet<MapperPropertyTreeModel> PropertyItems { get; set; } = [];
    private string MapperName { get; set; } = "";
    private string GetCursor(TreeItemData<PropertyTreeData> context) =>
        context.HasChildren ? "cursor:pointer" : "cursor:default";
    public IReadOnlyCollection<TreeItemData<PropertyTreeData>>? PropertyTreeItems { get; set; }

    protected override void OnInitialized()
    {
        ClientService.OnReadExceptionHandler(OnReadExceptionOccurredHandler);
        ClientService.OnMapperIsUnloaded += OnMapperUnloadedHandler;
        var metaResult = ClientService.GetMetaData();
        if (metaResult.IsSuccess)
            MapperName = $"{metaResult.ResultValue?.GameName}";
        GetTreeData();
        base.OnInitialized();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (!firstRender) return;
    }
    private void GetTreeData(int depth = 0)
    {
        var propsResult = ClientService.GetPropertiesTree();
        if (!propsResult.IsSuccess || propsResult.ResultValue is null) return;
        PropertyTreeItems = propsResult
            .ResultValue;
    }
    private string SetIcon(TreeItemData<PropertyTreeData> model)
    {
        return model.HasChildren
            ? (model.Expanded ? Icons.Material.Filled.FolderOpen : Icons.Material.Filled.Folder)
            : Icons.Material.Filled.Folder;
    }
    private static int TextSize(int len) => len * 8;

    private void Clear()
    {
        MapperName = "";
        PropertyTreeItems = [];
        //PropertyItems = [];
        ChangeNotificationService.NotifyDataChanged();
    }

    private async void OnMapperUnloadedHandler()
    {
        await InvokeAsync(Clear);
    }
    private async void OnReadExceptionOccurredHandler()
    {
        await InvokeAsync(() =>
        {
            Clear();
            StateHasChanged();
            try
            {
                NavService?.Navigate(NavigationService.Pages.MapperManager);
            }
            catch (Exception _)
            {
                // ignored
            }
        });
    }
}