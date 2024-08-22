using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain;
public class PropertyChangedEventArgs(List<IPokeAByteProperty> changedProperties) : EventArgs
{
    public List<IPokeAByteProperty> ChangedProperties { get; } = changedProperties;
}
public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);