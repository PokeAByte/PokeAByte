namespace GameHook.Domain.Interfaces
{
    public interface IClientNotifier
    {
        Task SendInstanceReset();
        Task SendMapperLoaded(IGameHookMapper mapper);
        Task SendError(IProblemDetails problemDetails);
        Task SendPropertiesChanged(IEnumerable<IGameHookProperty> properties);
    }
}
