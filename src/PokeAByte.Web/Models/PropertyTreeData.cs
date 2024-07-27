using MudBlazor;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;

namespace PokeAByte.Web.Models;

public sealed class PropertyTreeData : TreeItemData<PropertyTreeModel>
{
    public int Depth { get; init; }
    public bool IsParentExpanded => Parent?.Expanded ?? false;
    public TreeItemData<PropertyTreeData>? Parent { get; init; }

    public PropertyTreeData(string text, string icon, PropertyTreeModel property, int depth)
        : base(property)
    {
        Text = text;
        Icon = icon;
        Depth = depth;
    }
    private int GetMaxLength()
    {
        if (HasChildren)
        {
            return Children
                .Aggregate(0, (max, current) =>
                    Math.Max(max, current.Text?.Length ?? 0));
        }
        return Text?.Length ?? 0;
    }
}
public class PropertyTreeModel
{
    public required string Name { get; set; }
    public PropertyModel? Property { get; set; }
    public required Guid MapperId { get; set; }
    public required string MapperName { get; set; }
    public required string FullPath { get; set; }
    public bool IsPropertyExpanded { get; set; }
}
public static class PropertyTreeDataExtensions
{
    public static TreeItemData<PropertyTreeData>? CloneWithoutChildren(this TreeItemData<PropertyTreeData> treeItemData)
    {
        if (treeItemData.Value?.Value is null)
            return null;
        
        var treeVal = new PropertyTreeData(
            treeItemData.Value.Text ?? "",
            treeItemData.Value.Icon ?? "",
            treeItemData.Value.Value,
            treeItemData.Value.Depth)
        {
            Children = treeItemData.Value.HasChildren ? [new TreeItemData<PropertyTreeModel>()] : [],
            Expandable = treeItemData.Value.Expandable,
            Expanded = treeItemData.Value.Expanded,
            Parent = treeItemData.Value.Parent
        }; 
        
        var newTreeItemData = new TreeItemData<PropertyTreeData>
        {
            Children = treeItemData.HasChildren ? [new TreeItemData<PropertyTreeData>()] : [],
            Expandable = treeItemData.Expandable,
            Text = treeItemData.Text,
            Icon = treeItemData.Icon,
            Value = treeVal,
        };
        return newTreeItemData;
    }
    public static void AddProperty(this List<TreeItemData<PropertyTreeData>> tree,
        PropertyModel model, 
        MapperMetaModel metadata)
    {
        var paths = model.Path.Split('.');
        if (paths.Length == 0)
            throw new InvalidOperationException("Paths cannot be empty.");
        //Check if tree is empty
        if (tree.Count == 0)
        {
            //create new entry
            var newPropEntry = new PropertyTreeData(paths[0],
                    "", 
                    new PropertyTreeModel
                    {
                        Name = paths[0],
                        Property = paths.Length == 1
                            ? model
                            : null,
                        MapperId = metadata.Id,
                        MapperName = metadata.GameName,
                        FullPath = paths[0],
                    },
                    0);
            var newEntry = new TreeItemData<PropertyTreeData>()
            {
                Text = paths[0],
                Icon = "",
                Value = newPropEntry,
                Children = []
            };
            tree.Add(newEntry);
        }
        //Get the tree we want to add to
        var currentTreePath = tree
            .FirstOrDefault(x => x.Text == paths[0]);
        if (currentTreePath is null)
        {
            //Node was not found with this given path, create it.
            var newPropEntry = new PropertyTreeData(paths[0],
                    "", 
                    new PropertyTreeModel
                    {
                        Name = paths[0],
                        Property = paths.Length == 1
                            ? model
                            : null,
                        MapperId = metadata.Id,
                        MapperName = metadata.GameName,
                        FullPath = paths[0]
                    },
                    0);
            currentTreePath = new TreeItemData<PropertyTreeData>()
            {
                Text = paths[0],
                Icon = "",
                Value = newPropEntry,
                Children = []
            };
            //Add the new node
            tree.Add(currentTreePath);
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
            
            //Find the children with the name of the next path...
            var child = currentTreePath
                .Children?
                .FirstOrDefault(x => x.Text == paths[index]);
            if (child is null)
            {
                //The child does not exist for this given path, create it.
                var newPropEntry = new PropertyTreeData(paths[0],
                    "", 
                    new PropertyTreeModel
                    {
                        Name = paths[index],
                        Property = paths.Length == index + 1 ? 
                            model : null,
                        MapperId = metadata.Id,
                        MapperName = metadata.GameName,
                        FullPath = paths[0]
                    },
                    index)
                {
                    Parent = currentTreePath
                };
                child = new TreeItemData<PropertyTreeData>()
                {
                    Text = paths[index],
                    Icon = "",
                    Value = newPropEntry,
                };
                //Add the child to the current node
                currentTreePath.Children ??= [];
                currentTreePath.Children.Add(child);
            }
            //Move to the child node 
            currentTreePath = child;
        }
    }
}
