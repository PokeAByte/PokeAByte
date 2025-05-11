using System.Runtime.InteropServices;

namespace PokeAByte.Protocol;

[StructLayout(LayoutKind.Sequential)]
public struct ReadBlock
{
    /// <summary>
    /// Position of the block in the MMF.
    /// </summary>
    public uint Position;
    public uint GameAddress;
    public int Length;
}
