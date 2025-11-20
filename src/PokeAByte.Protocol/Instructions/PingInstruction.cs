using System.Runtime.InteropServices;

namespace PokeAByte.Protocol;

[StructLayout(LayoutKind.Explicit)]
public struct PingInstruction
{
    [FieldOffset(0)]
    public Metadata Metadata = new Metadata(Instructions.PING, 0x00);

    public PingInstruction()
    {
    }

    public byte[] GetByteArray()
    {
        var result = new byte[Metadata.HEADER_LENGTH];
        Metadata.CopyTo(result);
        return result;
    }
}