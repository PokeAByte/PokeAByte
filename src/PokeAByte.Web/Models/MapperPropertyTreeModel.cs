﻿using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;

namespace PokeAByte.Web.Models;

public class MapperPropertyTreeModel
{
    public event EventHandler? PropertyChangedEvent;
    public event Action<MapperPropertyTreeModel>? PropertyExpandedEvent;
    public required string Name { get; set; }
    public int Depth { get; set; }
    private bool _isExpanded = false;
    public bool ShouldTriggerExpandedAction { get; set; } = true;
    public int CurrentCount { get; set; }
    private int _totalTake = 150;

    //public event EventHandler TreeExpanded;
    public bool IsExpanded 
    { 
        get => _isExpanded;
        set
        {
            _isExpanded = value;
            if (_isExpanded)
            {
                UpdateDisplayedChildren(this);
                UpdateDisplayWidth(this);
            }
            else
            {
                MinimizeChildren();
            }
            if(ShouldTriggerExpandedAction)
                PropertyExpandedEvent?.Invoke(this);
        }
    }
    public bool IsPropertyExpanded { get; set; }
    public PropertyModel? Property { get; init; }
    public MapperPropertyTreeModel? Parent { get; init; }
    public bool IsParentExpanded => Parent?.IsExpanded ?? false;
    public HashSet<MapperPropertyTreeModel> Children { get; set; } = [];
    public HashSet<MapperPropertyTreeModel> DisplayedChildren { get; set; } = [];
    public int Width { get; set; } = 125;
    public bool HasChildren { get; set; }
    public required Guid MapperId { get; set; }
    public required string MapperName { get; set; }
    public required string FullPath { get; set; }
    public bool HasMoreItems => CurrentCount + _totalTake < Children.Count;
    public int Index { get; set; } = 0;
    private void MinimizeChildren()
    {
        foreach (var child in 
                 Children.Where(child => child.IsExpanded))
        {
            child.IsExpanded = false;
        }
    }
    private int GetMaxLength()
    {
        if (HasChildren)
        {
            return Children
                .Aggregate(0, (max, current) =>
                    Math.Max(max, current.Name.Length));
        }
        return Name.Length;
    }

    private static void UpdateDisplayWidth(MapperPropertyTreeModel model)
    {
        if (!model.HasChildren) return;
        foreach (var dc in model.DisplayedChildren)
        {
            dc.Width = model.GetMaxLength();
        }
    }
    public static void UpdateOpenedDisplayedChildren(MapperPropertyTreeModel model)
    {
        foreach (var child in model.Children.Where(x => x._isExpanded))
        {
            UpdateOpenedDisplayedChildren(child);
            UpdateDisplayedChildren(child);
        }
    }

    public static void AddMoreDisplayedItems(MapperPropertyTreeModel model)
    {
        if (model.Parent is null || !model.Parent.HasMoreItems) return;
        var parent = model.Parent;
        var items = parent.Children
            .Skip(parent.CurrentCount)
            .Take(model._totalTake)
            .ToHashSet();
        foreach (var item in items)
        {
            parent.DisplayedChildren.Add(item);
        }
        parent.CurrentCount += model._totalTake;
    }
    public static void UpdateDisplayedChildren(MapperPropertyTreeModel model)
    {
        if (model.DisplayedChildren.Count > 1)
            return;
        /*if(!model.HasMoreItems)
            return;*/
        model.Children = model.Children.OrderBy(x => x.Index).ToHashSet();
        model.DisplayedChildren = model.Children.Skip(model.CurrentCount).Take(model._totalTake).ToHashSet();
        model.CurrentCount += model._totalTake;
        //If we have more items, we should let the user know
        if (model.HasMoreItems)
            model.DisplayedChildren.Add(new MapperPropertyTreeModel
            {
                Name = "Click to load more items.",
                MapperId = default,
                MapperName = "",
                FullPath = "",
                IsLoadMoreItemsEntry = true,
                Parent = model
            });
        //Lie to the tree view and make it think we have a list of children, we will load them
        //in later when we need them
        foreach (var dc in model.DisplayedChildren.Where(x => x.HasChildren))
        {
            dc.DisplayedChildren = [new MapperPropertyTreeModel
                {
                    Name = "",
                    MapperId = default,
                    MapperName = "",
                    FullPath = ""
                }
            ];
        }
    }

    public bool IsLoadMoreItemsEntry { get; set; }

    public void UpdateProperty(IPokeAByteProperty prop)
    {
        Property?.UpdatePropertyModel(prop);
        PropertyChangedEvent?.Invoke(this, EventArgs.Empty);
    }
}

public class MapperPropertyTree : IDisposable
{
    public HashSet<MapperPropertyTreeModel> Tree { get; } = [];

    public void AddProperty(PropertyModel model, 
        MapperMetaModel metadata,
        Action<MapperPropertyTreeModel> onExpanded)
    {
        var paths = model.Path.Split('.');
        if (paths.Length == 0)
            throw new InvalidOperationException("Paths cannot be empty.");
        //Check if tree is empty
        if (Tree.Count == 0)
        {
            //create new entry
            var newEntry = new MapperPropertyTreeModel
            {
                Name = paths[0],
                Property = paths.Length == 1
                    ? model
                    : null,
                Depth = 0,
                MapperId = metadata.Id,
                MapperName = metadata.GameName,
                FullPath = paths[0]
            };
            newEntry.PropertyExpandedEvent += onExpanded;
            Tree.Add(newEntry);
        }
        //Get the tree we want to add to
        var currentTreePath = Tree
            .FirstOrDefault(x => x.Name == paths[0]);
        if (currentTreePath is null)
        {
            //Node was not found with this given path, create it.
            currentTreePath = new MapperPropertyTreeModel
            {
                Name = paths[0],
                Property = paths.Length == 1 ? model : null,
                Depth = 0,
                MapperId = metadata.Id,
                MapperName = metadata.GameName,
                FullPath = paths[0]
            };
            currentTreePath.PropertyExpandedEvent += onExpanded;
            //Add the new node
            Tree.Add(currentTreePath);
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
                .Children
                .FirstOrDefault(x => x.Name == paths[index]);
            if (child is null)
            {                
                var last = currentTreePath.Children
                    .OrderBy(x => x.Index)
                    .Select(x => x.Index)
                    .LastOrDefault();
                //The child does not exist for this given path, create it.
                child = new MapperPropertyTreeModel
                {
                    Name = paths[index],
                    Depth = index,
                    Property = paths.Length == index + 1 ? 
                        model : null,
                    MapperId = metadata.Id,
                    MapperName = metadata.GameName,
                    FullPath = string.Join(".", paths[..(index+1)]),
                    Parent = currentTreePath,
                    Index = last + 1
                };
                child.PropertyExpandedEvent += onExpanded;
                //Add the child to the current node
                currentTreePath.Children.Add(child);
                currentTreePath.HasChildren = currentTreePath.Children.Count > 0;
            }
            else
            {
                var last = currentTreePath.Children
                    .OrderByDescending(x => x.Index)
                    .Select(x => x.Index)
                    .LastOrDefault();
                child.Index = last + 1;
            }
            //Move to the child node 
            currentTreePath = child;
        }
    }
    public void Dispose()
    {
        Tree.Clear();
    }

    public void UpdateProperty(IPokeAByteProperty prop, EventHandler? propertyUpdatedEvent)
    {
        var paths = prop.Path.Split('.');
        if (paths.Length == 0) return;
        var currentTree = Tree
            .FirstOrDefault(x => x.Name == paths[0]);
        if(currentTree is null)
            return;
        for (var i = 1; i < paths.Length; ++i)
        {
            currentTree = currentTree?
                .Children
                .FirstOrDefault(x => x.Name == paths[i]);
        }

        if (currentTree is not null)
        {
            currentTree.PropertyChangedEvent += propertyUpdatedEvent;
            currentTree.UpdateProperty(prop);
        }
    }
}

