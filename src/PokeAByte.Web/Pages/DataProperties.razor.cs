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
    private HashSet<MapperPropertyTreeModel> PropertyItems { get; set; } = [];
    private string MapperName { get; set; } = "";
    private bool _isLoadingScroll = false;
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ClientService.OnReadExceptionHandler(OnReadExceptionOccurredHandler);
        ClientService.OnMapperIsUnloaded += OnMapperUnloadedHandler;
        var metaResult = ClientService.GetMetaData();
        if (metaResult.IsSuccess)
            MapperName = $"{metaResult.ResultValue?.GameName}";
        var propsResult = ClientService.GetPropertiesHashSet();
        if (!propsResult.IsSuccess || propsResult.ResultValue is null) return;
        PropertyItems = propsResult.ResultValue;
        foreach (var item in PropertyItems)
        {
            MapperPropertyTreeModel.UpdateOpenedDisplayedChildren(item);
        }
    }
    private string SetIcon(MapperPropertyTreeModel model)
    {
        return model.HasChildren
            ? (model.IsExpanded ? Icons.Material.Filled.FolderOpen : Icons.Material.Filled.Folder)
            : "";
    }
    private static int TextSize(int len) => len * 8;

    private void Clear()
    {
        MapperName = "";
        PropertyItems = [];
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

    private void OnScroll()
    {
        //get all loaded properties where the parent is expanded
        var loadedProps = PropertyItems
            .Where(x => x.HasMoreItems)
            .ToList();
        if (loadedProps.Count == 0)
        {
            _isLoadingScroll = false;
            return;
        }

        _isLoadingScroll = true;
        foreach (var item in loadedProps)
        {
            //MapperPropertyTreeModel.UpdateDisplayedChildren(item, true);
        }

        _isLoadingScroll = false;
    }

    private void LoadMoreItems(MapperPropertyTreeModel context)
    {
        MapperPropertyTreeModel.AddMoreDisplayedItems(context);
        var parent = context.Parent;
        if (parent is not null)
        {
            //parent.DisplayedChildren.RemoveWhere(x => x.IsLoadMoreItemsEntry);

            if (parent.HasMoreItems)
            {
                context.Index = parent
                    .DisplayedChildren
                    .OrderBy(x => x.Index)
                    .Select(x => x.Index)
                    .LastOrDefault() + 1;
                parent.DisplayedChildren = parent.DisplayedChildren
                    .OrderBy(x => x.Index)
                    .ToHashSet();
            }
        }
        StateHasChanged();
    }
}