using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;
using PokeAByte.Web.Services.Navigation;
using PokeAByte.Web.Services.Notifiers;
using PokeAByte.Web.Services.Properties;

namespace PokeAByte.Web.Components.PropertyManager;

public partial class PropertyTreeView : ComponentBase
{
    [Inject] private PropertyService PropertyService { get; set; }
    [Inject] private MapperClientService ClientService { get; set; }
    [Inject] private NavigationService? NavService { get; set; }
    [Inject] private ChangeNotificationService ChangeNotificationService { get; set; }

    private const int CountIncrease = 150;
    private int _maxCount = CountIncrease;
    private string _mapperName = "";
    private void Clear()
    {
        _mapperName = "";
        PropertyService.ResetTree();
        ChangeNotificationService.NotifyDataChanged();
    }
    private string SetIcon(TreeItemData<PropertyTreeItem> context)
    {        
        return context.HasChildren
            ? (context.Expanded ? Icons.Material.Filled.FolderOpen : Icons.Material.Filled.Folder)
            : Icons.Material.Filled.Folder;
    }

    private static int TextSize(int? len) => len * 8 ?? 125;
    
    protected override void OnInitialized()
    {
        ClientService.OnReadExceptionHandler(OnReadExceptionOccurredHandler);
        ClientService.OnMapperIsUnloaded += OnMapperUnloadedHandler;
        var metaResult = ClientService.GetMetaData();
        if (metaResult.IsSuccess)
            _mapperName = $"{metaResult.ResultValue?.GameName}";
        var result = PropertyService.GenerateTree();
        StateHasChanged();
        if (result.IsSuccess)
        {
            //Todo: show user success
        }
        else
        {
            //Todo: show user error
        }
        base.OnInitialized();
    }

    private void OnTreeItemClick(TreeItemData<PropertyTreeItem> context)
    {
        if (!context.Expandable)
            return;
        context.Expanded = !context.Expanded;
        if (context.HasChildren)
        {
            //enable children
            context.Children!.ForEach(x =>
            {
                if (x is not PropertyTreePresenter presenter) return;
                presenter.IsDisabled = !context.Expanded;
            });
        }
        if(context is PropertyTreePresenter pc)
            PropertyService.SaveOpenProperty(pc);
        StateHasChanged();
    }
    private void OnClickLoadEntries(PropertyTreePresenter presenter)
    {
        _maxCount += CountIncrease;
        if (presenter.Children is not null && presenter.HasChildren)
        {
            foreach (var child in presenter
                         .Children
                         .Where(x => x.Value?.CurrentCount <= _maxCount))
            {
                if (child is not PropertyTreePresenter childPresenter)
                    continue;
                childPresenter.IsDisabled = !childPresenter.Expanded;
            }
        }
        StateHasChanged();
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