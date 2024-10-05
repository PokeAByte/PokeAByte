using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain;
public class PropertyChangedEventArgs(IList<IPokeAByteProperty> changedProperties) : EventArgs
{
    public IList<IPokeAByteProperty> ChangedProperties { get; } = changedProperties;
}
public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);