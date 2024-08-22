namespace PokeAByte.Web.Services.Notifiers;

public class PropertyUpdateService(ILogger<PropertyUpdateService> logger)
{
    public Dictionary<string, EventHandler<PropertyUpdateEventArgs>> EventHandlers { get; set; } = [];

    public void NotifyChanges(string path)
    {
        var gotValue = EventHandlers.TryGetValue(path, out var eventHandler);
        if (gotValue)
        {
            eventHandler?.Invoke(this, new PropertyUpdateEventArgs
            {
                Path = path
            });
        }
    }
}

public class PropertyUpdateEventArgs : EventArgs
{
    public required string Path { get; set; }
}