namespace GameHook.Domain.Interfaces
{
    public interface IMapperUpdateManager
    {
        Task<bool> CheckForUpdates();
    }
}
