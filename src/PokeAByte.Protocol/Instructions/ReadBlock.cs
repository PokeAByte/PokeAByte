using System.Runtime.InteropServices;

namespace PokeAByte.Protocol;

/// <summary>
/// Describes a block of game memory to be read and provided by the protocol server.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ReadBlock
{
    /// <summary>
    /// Position of the first byte of the block in the MMF.
    /// </summary>
    public uint Position;

    /// <summary>
    /// The address of the blocks first byte in the game memory. 
    /// </summary>
    public uint GameAddress;

    /// <summary>
    /// The number of bytes to read from the <see cref="GameAdress"/> (inclusive).
    /// </summary>
    public int Length;
}
