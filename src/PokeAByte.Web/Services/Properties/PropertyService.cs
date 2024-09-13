using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;
using PokeAByte.Web.Services.Notifiers;

namespace PokeAByte.Web.Services.Properties;

public class PropertyService(MapperClientService clientService, 
    ILogger<PropertyService> logger,
    MapperSettingsService mapperSettings,
    PropertyUpdateService propertyUpdateService)
{
    private MapperClient Client => clientService.Client;
    private List<TreeItemData<PropertyTreeItem>> _propertyTree = [];
    public IReadOnlyList<TreeItemData<PropertyTreeItem>> PropertyTree => 
        _propertyTree.AsReadOnly();

    public Result GenerateTree()
    {
        _propertyTree = [];
        var mapperMetadata = Client.GetMetaData();
        if (mapperMetadata is null)
            return Result.Failure(Error.FailedToLoadMetaData, "Failed to get metadata.");
        var properties = Client.GetProperties();
        if (properties is null)
            return Result.Failure(Error.NoMapperPropertiesFound, "Failed to get properties.");
        foreach (var property in properties)
        {
            var addedProp = _propertyTree.AddProperty(property, mapperMetadata);
            if (addedProp?.Value?.PropertyModel is null) continue;
            var reference = addedProp.Value.PropertyModel.Reference;
            if(string.IsNullOrEmpty(reference)) continue;
            var glossary = GetGlossaryByReferenceKey(reference);
            addedProp.Value.PropertyModel.GlossaryReference = glossary;
        }

        OpenProperties();
        return Result.Success();
    }
    private Dictionary<ulong, string> GetGlossaryByReferenceKey(string reference)
    {
        var glossaryResult = clientService.GetGlossaryByReferenceKey(reference);
        if (!glossaryResult.IsSuccess)
            //todo log
            return [];
        var glossaryItems = glossaryResult.ResultValue;
        return glossaryItems?
            .Select(g => new KeyValuePair<ulong, string>(g.Key, g.Value?.ToString() ?? ""))
            .GroupBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.First().Value) ?? [];
    }
    private void OpenProperties()
    {
        var openProperties = mapperSettings.GetMapperModelProperties();
        if (openProperties is null) return;
        foreach (var openProperty in openProperties)
        {
            var pathSplit = openProperty.PropertyPath.Split('.');
            var currentBranch = _propertyTree
                .FirstOrDefault(x => x.Text == pathSplit[0]);
            //Found the first branch
            if (currentBranch is not null)
            {
                if (currentBranch.Value?.FullPath == openProperty.PropertyPath)
                {
                    //Found the entire branch, open it
                    currentBranch.Expanded = openProperty.IsExpanded;
                    if (currentBranch is PropertyTreePresenter presenter)
                    {
                        presenter.IsDisabled = !openProperty.IsExpanded;
                    }
                }
                else
                {
                    //check the children
                    foreach (var path in pathSplit)
                    {
                        while (currentBranch?.Text != path)
                        {
                            if (currentBranch?.HasChildren is true)
                            {
                                foreach (var child in currentBranch.Children!)
                                {
                                    if (child is PropertyTreePresenter c &&
                                        child.Text?
                                        .Contains(path, StringComparison.InvariantCultureIgnoreCase) is true)
                                    {
                                        c.IsDisabled = false;
                                    }

                                    if (child.Text == path)
                                    {
                                        currentBranch = child;
                                        currentBranch.Expanded = true;
                                    }
                                }
                            }
                        }

                        if (currentBranch.Text == path)
                        {
                            currentBranch.Expanded = openProperty.IsExpanded;
                            if (currentBranch is PropertyTreePresenter presenter)
                            {
                                presenter.IsDisabled = false;
                                presenter.DisableChildren(false);
                            }
                        }
                    }
                }
            }
        }
    }

    public void SaveOpenProperty(PropertyTreePresenter property)
    {
        mapperSettings.OnPropertyExpandedHandler(property);
    }
    public void ResetTree()
    {
        _propertyTree = [];
    }

    public void SetEventHandlers(EventHandler<PropertyUpdateEventArgs> handlePropertyUpdate)
    {
        var clientProperties = Client.GetProperties();
        if(clientProperties is null)
            return;
        foreach (var property in clientProperties)
        {
            if (!propertyUpdateService.EventHandlers.ContainsKey(property.Path))
            {
                propertyUpdateService.EventHandlers.TryAdd(property.Path, handlePropertyUpdate);
            }
        }
    }

    public void RemoveEventHandlers()
    {
        var clientProperties = Client.GetProperties();
        if(clientProperties is null)
            return;
        foreach (var property in clientProperties)
        {
            if (propertyUpdateService.EventHandlers.ContainsKey(property.Path)) 
            {
                propertyUpdateService.EventHandlers.Remove(property.Path);
            }
        }
    }

    public void UpdateProperty(string path)
    {
        //Find path
        var property = _propertyTree.FindWithPath(path);
        var updatedModel = Client.GetPropertyByPath(path);
        if (updatedModel is null || property is null)
            return;
        property.PropertyModel?.UpdateFromPropertyModel(updatedModel);
    }
}