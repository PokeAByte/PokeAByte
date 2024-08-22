namespace PokeAByte.Domain.Interfaces
{
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
        Task<bool> TestConnection();

        Task<Dictionary<uint, byte[]>> ReadBytes(IEnumerable<MemoryAddressBlock> blocks);

        Task WriteBytes(uint startingMemoryAddress, byte[] values);
    }

    public interface IBizhawkMemoryMapDriver : IPokeAByteDriver { }
    public interface IRetroArchUdpPollingDriver : IPokeAByteDriver { }
    public interface IStaticMemoryDriver : IPokeAByteDriver
    {
        Task SetMemoryFragment(string filename);
    }
}