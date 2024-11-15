namespace PokeAByte.Domain.Interfaces
{
    public record BlockData(MemoryAddress Start, byte[] Data);

    /// <summary>
    /// Driver interface for interacting with a emulator.
    /// 
    /// - Driver should not log anything above LogDebug.
    /// - Any errors encountered should be thrown as exceptions.
    /// </summary>
    public interface IPokeAByteDriver
    {
        string ProperName { get; }

        int DelayMsBetweenReads { get; }

        Task EstablishConnection();
        Task Disconnect();
        Task<bool> TestConnection();

        Task<BlockData[]> ReadBytes(IList<MemoryAddressBlock> blocks);

        Task WriteBytes(uint startingMemoryAddress, byte[] values, string? path = null);
    }

    public interface IBizhawkMemoryMapDriver : IPokeAByteDriver { }
    public interface IRetroArchUdpPollingDriver : IPokeAByteDriver { }
    public interface IStaticMemoryDriver : IPokeAByteDriver
    {
        Task SetMemoryFragment(string filename);
    }
}