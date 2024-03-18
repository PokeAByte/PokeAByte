namespace GameHook.Domain.Interfaces
{
    /// <summary>
    /// Driver interface for interacting with a emulator.
    /// 
    /// - Driver should not log anything above LogDebug.
    /// - Any errors encountered should be thrown as exceptions.
    /// </summary>
    public interface IGameHookDriver
    {
        string ProperName { get; }

        int DelayMsBetweenReads { get; }

        Task EstablishConnection();

        Task<Dictionary<uint, byte[]>> ReadBytes(IEnumerable<MemoryAddressBlock> blocks);

        Task WriteBytes(uint startingMemoryAddress, byte[] values);
    }

    public interface IBizhawkMemoryMapDriver : IGameHookDriver { }
    public interface IRetroArchUdpPollingDriver : IGameHookDriver { }
    public interface IStaticMemoryDriver : IGameHookDriver
    {
        Task SetMemoryFragment(string filename);
    }
}