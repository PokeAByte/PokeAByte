using PokeAByte.Domain.Models;

namespace PokeAByte.Domain.Interfaces;

public class BlockData
{
    public MemoryAddress Start { get; set; }
    public byte[] Data { get; set; }

    public BlockData(uint start, byte[] data)
    {
        Start = start;
        Data = data;
    }
}

/// <summary>
/// Driver interface for interacting with a emulator.
/// 
/// - Driver should not log anything above LogDebug.
/// - Any errors encountered should be thrown as exceptions.
/// </summary>
public interface IPokeAByteDriver
{
    /// <summary>
    /// The proper name of the emulator (or protocol) the driver is for.
    /// </summary>
    string ProperName { get; }

    /// <summary>
    /// How many milliseconds the Poke-A-Byte instance should wait in between calls to <see cref="ReadBytes"/>.
    /// </summary>
    int DelayMsBetweenReads { get; }

    /// <summary>
    /// Connect the driver to the emulator.
    /// </summary>
    /// <returns> An awaitable task. </returns>
    Task EstablishConnection();

    /// <summary>
    /// Disconnect the driver from the emulator.
    /// </summary>
    /// <returns> An awaitable task. </returns>
    Task Disconnect();

    /// <summary>
    /// Read a number of memory blocks from the emulator. 
    /// </summary>
    /// <param name="transferBlocks"> The blocks to read and for transporting the data. </param>
    /// <remarks> 
    /// The memory bytes are written back into the <see cref="BlockData.Data"/> byte array of each block in the
    /// <paramref name="transferBlocks"> parameter.
    /// </remarks>
    /// <returns> An awaitable task. </returns>
    ValueTask ReadBytes(BlockData[] transferBlocks);

    /// <summary>
    /// Instruct the emulator to write bytes into the games memory.
    /// </summary>
    /// <param name="startingMemoryAddress"> The starting address for the write. </param>
    /// <param name="values"> An array of bytes that the emulator should write. </param>
    /// <param name="path"> Currently unused. </param>
    /// <returns> An awaitable task. </returns>
    ValueTask WriteBytes(uint startingMemoryAddress, byte[] values, string? path = null);

    /// <summary>
    /// Check if the driver can connect to it's target emulator.
    /// </summary>
    /// <param name="appSettings"> General application settings. </param>
    /// <returns> True if the driver was able to connect. </returns>
    static virtual Task<bool> Probe(AppSettings appSettings) => Task.FromResult(false);
}

public interface IBizhawkMemoryMapDriver : IPokeAByteDriver { }
public interface IRetroArchUdpPollingDriver : IPokeAByteDriver { }
public interface IStaticMemoryDriver : IPokeAByteDriver
{
    Task SetMemoryFragment(string filename);
}