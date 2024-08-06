using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;
using PokeAByte.Web.Services.Navigation;
using PokeAByte.Web.Services.Notifiers;
using PokeAByte.Web.Services.Properties;

namespace PokeAByte.Web.Components.PropertyManager;

public partial class PropertyTreeView : ComponentBase, IDisposable
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

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
        {
            
        }
    }

    private void OnTreeItemClick(TreeItemData<PropertyTreeItem> context)
    {
        if (!context.Expandable)
            return;
        context.Expanded = !context.Expanded;
        //Make sure this isn't disabled
        if (context is PropertyTreePresenter p)
        {
            p.IsDisabled = false;
        }
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
    private string GetWidth(PropertyTreePresenter context, string additionalInfo = "")
    {
        List<TreeItemData<PropertyTreeItem>>? children;
        //Get the parent
        if (context.Parent is not null)
        {
            children = context.Parent.Children;
        }
        else
        {
            //This is the root nodes just get a list of root nodes
            children = PropertyService
                .PropertyTree
                .Where(x => x.HasChildren)
                .ToList();
        }
        //Failed to find children just make the length the size of the entry (8*16 ~= 125 px)
        if (children is null) return (context.Text?.Length ?? 16 * 8).ToString();
        //Iterate through the children and get the largest text size
        var length = children
            .Aggregate(0, (max, current) =>
                Math.Max(max, current.Text?.Length ?? 16)) * 10;
        if (length < 75) length = 75;
        if (!string.IsNullOrWhiteSpace(additionalInfo))
        {
            length += additionalInfo.Length * 10;
        }
        return length.ToString();
    }

    private string GetAdditionalInfo(TreeItemData<PropertyTreeItem> context)
    {
        var intConvert = int.TryParse(context.Text, out var result);
        if (!intConvert) return "";
        var firstChild =
            (context.HasChildren ? 
                context.Children?.FirstOrDefault(x => x.Value?.Name is "species") : 
                null) ?? (context.HasChildren ? 
                context.Children?.First() : 
                null);
        if (firstChild?.Value?.PropertyModel == null) return "";
        return string.IsNullOrWhiteSpace(firstChild.Value.PropertyModel.Value?.ToString()) ? "" : $" ({firstChild.Value.PropertyModel.Value})";
    }

    private string GetLength(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "0";
        var len = value.Length;
        return len * 8 < 17 ? "15" : (len * 8).ToString();
    }

    public void Dispose()
    {
        PropertyService.ResetTree();
    }

    public async Task RefreshParent()
    {
        await InvokeAsync(StateHasChanged);
    }
}