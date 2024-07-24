namespace PokeAByte.Domain.Interfaces
{
    public interface IClientNotifier
    {
        Task SendInstanceReset();
        Task SendMapperLoaded(IPokeAByteMapper mapper);
        Task SendError(IProblemDetails problemDetails);
        Task SendPropertiesChanged(IEnumerable<IPokeAByteProperty> properties);
        
        public event PropertyChangedEventHandler? PropertyChangedEvent;
    }
}
