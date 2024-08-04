using MudBlazor;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;

namespace PokeAByte.Web.Models;

public sealed class PropertyTreePresenter : TreeItemData<PropertyTreeItem>
{
    public bool IsDisabled { get; set; }
    public int LongestTextLength { get; set; } 
    public PropertyTreePresenter? Parent { get; set; }
    public PropertyTreePresenter(string text, 
        string icon,
        Guid mapperId,
        string mapperName,
        string fullPath,
        PropertyTreePresenter? parent = null,
        PropertyModel? property = null) : 
        base(new PropertyTreeItem(text,
            mapperId,
            mapperName,
            fullPath,
            property))
    {
        Text = text;
        Parent = parent;
        LongestTextLength = text.Length;
        Icon = icon;
    }
    public int GetMaxLength()
    {
        if (HasChildren)
        {
            return Children?
                .Aggregate(0, (max, current) =>
                    Math.Max(max, current.Text?.Length ?? 0)) ?? 0;

        }
        return Text?.Length ?? 0;
    }

    public void DisableChildren(bool presenterIsDisabled)
    {
        if (!HasChildren) return;
        foreach (var child in Children!)
        {
            if(child is PropertyTreePresenter p)
                p.IsDisabled = presenterIsDisabled;
        }
    }
}

public record PropertyTreeItem(string Name,
    Guid MapperId,
    string MapperName,
    string FullPath,
    PropertyModel? PropertyModel = null)
{
    public bool IsPropertyExpanded { get; set; }
    public int CurrentCount { get; private set; }

    public void SetCurrentCount(int count)
    {
        CurrentCount = count;
    }
}

public static class PropertyTreeExtensions
{
    public static void AddProperty(this List<TreeItemData<PropertyTreeItem>> tree, 
        PropertyModel property,
        MapperMetaModel metadata)
    {
        var paths = property.Path.Split('.');
        if (paths.Length == 0)
            throw new InvalidOperationException("Paths cannot be empty.");
        var currentNode = tree.FirstOrDefault(x => x.Text == paths[0]);
        if (currentNode is null)
        {
            //Tree or root is empty, create a new root
            tree.Add(new PropertyTreePresenter(
                paths[0],
                "",
                metadata.Id,
                metadata.GameName,
                property.Path,
                null,
                paths.Length == 1 ? property : null
            ));
            //Set current node to our new node we made
            currentNode = tree.First(x => x.Text == paths[0]);
            currentNode.Value?.SetCurrentCount(0);
        }
        //Iterate through the rest of the paths
        for (var index = 1; index < paths.Length; index++)
        {
            //So there are a couple of things going on here, we are first checking to see
            //if the current node for the previous path has a child with the name of 
            //the path we are iterating on currently then we do not need to do anything
            //other than moving into that child node. However, if it does not exist then
            //we need to create and add the new node to the tree before we can iterate into it.
            //We are also assuming that the last path will always be a property so if we are not
            //on the last path then we set the node's property to null and move on, otherwise
            //we set the property to be the parameter being passed in
            
            var child = currentNode.Children?
                .FirstOrDefault(x => x.Text == paths[index]);
            if (child is null)
            {
                //Child is null, so we should create a new one
                child = new PropertyTreePresenter(paths[index],
                    "",
                    metadata.Id,
                    metadata.GameName,
                    property.Path,
                    currentNode as PropertyTreePresenter,
                    paths.Length == index + 1 ? property : null);
                //If the child array doesn't exist just create it
                currentNode.Children ??= [];
                //Add the new child to the array of children
                currentNode.Children.Add(child);
            }
            //Make sure to disable all child nodes so we do not waste resources
            if (child is PropertyTreePresenter ptpChild)
            {
                ptpChild.IsDisabled = true;
            }
            child.Value?.SetCurrentCount(currentNode.Children?.Count ?? 0);
            
            //Move into the child node and repeat until we finish all paths
            currentNode = child;
        }
    }
}