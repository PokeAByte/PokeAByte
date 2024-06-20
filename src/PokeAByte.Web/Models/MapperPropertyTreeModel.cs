using GameHook.Domain.Models.Mappers;
using GameHook.Domain.Models.Properties;

namespace PokeAByte.Web.Models;

public class MapperPropertyTreeModel
{
    public string Name { get; set; }
    public int Depth { get; set; }
    private bool isExpanded = false;
    //public event EventHandler TreeExpanded;
    public bool IsExpanded { get => isExpanded;
        set
        {
            isExpanded = value;
            if (IsExpanded)
            {
                UpdateDisplayedChildren(this);
            }
        }
    }
    public bool IsPropertyExpanded { get; set; }
    public PropertyModel? Property { get; init; }
    public HashSet<MapperPropertyTreeModel> Children { get; set; } = [];
    public HashSet<MapperPropertyTreeModel> DisplayedChildren { get; set; } = [];
    public bool HasChildren { get; set; }

    public static void UpdateDisplayedChildren(MapperPropertyTreeModel model)
    {
        model.DisplayedChildren = model.Children;
        //Lazy load the next set of children
        foreach (var dc in model.DisplayedChildren.Where(x => x.HasChildren))
        {
            dc.DisplayedChildren = dc.Children;
        }
    }
    public static MapperPropertyTreeModel CreateFrom(MapperPropertyTreeModel model)
        => new()
        {
            Name = model.Name,
            Depth = model.Depth,
            IsExpanded = model.IsExpanded,
            Property = model.Property,
            HasChildren = model.Children.Count > 0,
            DisplayedChildren = [],
            Children = []
        };
}

public class MapperPropertyTree : IDisposable
{
    public HashSet<MapperPropertyTreeModel> Tree { get; } = [];

    public void AddProperty(PropertyModel model)
    {
        var paths = model.Path.Split('.');
        if (paths.Length == 0)
            throw new InvalidOperationException("Paths cannot be empty.");
        //Check if tree is empty
        if (Tree.Count == 0)
        {
            //create new entry
            Tree.Add(new MapperPropertyTreeModel
            {
                Name = paths[0],
                Property = paths.Length == 1 ? model : null,
                Depth = 0
            });
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
            };
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
                //The child does not exist for this given path, create it.
                child = new MapperPropertyTreeModel
                {
                    Name = paths[index],
                    Depth = index,
                    Property = paths.Length == index + 1 ? 
                        model : null
                };
                //Add the child to the current node
                currentTreePath.Children.Add(child);
                currentTreePath.HasChildren = currentTreePath.Children.Count > 0;
            }
            //Move to the child node 
            currentTreePath = child;
        }
    }
    public void Dispose()
    {
        Tree.Clear();
    }
}

