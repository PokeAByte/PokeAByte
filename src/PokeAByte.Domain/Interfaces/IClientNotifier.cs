namespace PokeAByte.Domain.Interfaces;

public interface IClientNotifier
{
    Task SendInstanceReset();
    Task SendMapperLoaded(IPokeAByteMapper mapper);
    Task SendError(IProblemDetails problemDetails);
    Task SendPropertiesChanged(IList<IPokeAByteProperty> properties);
}
