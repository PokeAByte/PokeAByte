using GameHook.Domain.Interfaces;

namespace GameHook.Domain;
public class PropertyChangedEventArgs(List<IGameHookProperty> changedProperties) : EventArgs
{
    public List<IGameHookProperty> ChangedProperties { get; } = changedProperties;
}
public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);