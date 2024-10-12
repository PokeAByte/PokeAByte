namespace PokeAByte.Domain.Interfaces
{
    /// <summary>
    /// Emulator memory data.
    /// </summary>
    /// <param name="Start"> The starting address of the memory read. </param>
    /// <param name="Data"> The bytes returned from the emulator. </param>
    public record BlockData(MemoryAddress Start, byte[] Data);

    /// <summary>
    /// Driver interface for interacting with a emulator.
    /// 
    /// - Driver should not log anything above LogDebug.
    /// - Any errors encountered should be thrown as exceptions.
    /// </summary>
    public interface IPokeAByteDriver
    {
        /// <summary>
        /// The proper name of the emulator the driver is for.
        /// </summary>
        string ProperName { get; }

        /// <summary>
        /// How many milliseconds the PokeAByte instance should wait in between calls to <see cref="ReadBytes"/>.
        /// </summary>
        int DelayMsBetweenReads { get; }

        Task EstablishConnection();

        /// <summary>
        /// Disconnect the driver from the emulator.
        /// </summary>
        /// <returns> An awaitable task. </returns>
        Task Disconnect();

        /// <summary>
        /// Checks whether a connection to the emulator can established.
        /// </summary>
        /// <returns> <see langword="true"/> if a connection has been made. Otherwise <see langword="false"/></returns>
        Task<bool> TestConnection();

        /// <summary>
        /// Read a number of memory blocks from the emulator.
        /// </summary>
        /// <remarks>
        /// Some drivers may not use the provided <paramref name="blocks"/> argument but instead specify their own
        /// memory ranges that they read from the emulator. 
        /// </remarks>
        /// <param name="blocks"> Which blocks to read. </param>
        /// <returns> An array of <see cref="BlockData"/> containing the read memory ranges. </returns>
        Task<BlockData[]> ReadBytes(IList<MemoryAddressBlock> blocks);

        /// <summary>
        /// Instruct the emulator to write bytes into the games memory.
        /// </summary>
        /// <param name="startingMemoryAddress"> The starting address for the write. </param>
        /// <param name="values"> An array of bytes that the emulator should write. </param>
        /// <param name="path"> Currently unused. </param>
        /// <returns> An awaitable task. </returns>
        Task WriteBytes(uint startingMemoryAddress, byte[] values, string? path = null);
    }

    public interface IBizhawkMemoryMapDriver : IPokeAByteDriver { }
    public interface IRetroArchUdpPollingDriver : IPokeAByteDriver { }
    public interface IStaticMemoryDriver : IPokeAByteDriver
    {
        Task SetMemoryFragment(string filename);
    }
}