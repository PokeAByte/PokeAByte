using MudBlazor;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Services.Properties;

public class PropertyService(MapperClientService clientService, 
    ILogger<PropertyService> logger,
    MapperSettingsService mapperSettings)
{
    private MapperClient Client => clientService.Client;
    private List<TreeItemData<PropertyTreeItem>> _propertyTree = [];
    public IReadOnlyList<TreeItemData<PropertyTreeItem>> PropertyTree => 
        _propertyTree.AsReadOnly();

    public Result GenerateTree()
    {
        var mapperMetadata = Client.GetMetaData();
        if (mapperMetadata is null)
            return Result.Failure(Error.FailedToLoadMetaData, "Failed to get metadata.");
        var properties = Client.GetProperties();
        if (properties is null)
            return Result.Failure(Error.NoMapperPropertiesFound, "Failed to get properties.");
        foreach (var property in properties)
        {
            _propertyTree.AddProperty(property, mapperMetadata);
        }

        OpenProperties();
        return Result.Success();
    }

    /*private void OpenProperties()
    {
        var properties = mapperSettings.GetMapperModelProperties();
        if (properties is null) return;
        foreach (var property in properties)
        {
            var pathSplit = property.PropertyPath.Split('.');
            var currentBranch = _propertyTree
                .FirstOrDefault(x => x.Text == pathSplit[0]);
            if (currentBranch is PropertyTreePresenter p)
            {
                p.Expanded = true;
                p.IsDisabled = false;
                while (currentBranch.Value?.FullPath != property.PropertyPath)
                {
                    
                }
            }
        }
    }*/
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
}