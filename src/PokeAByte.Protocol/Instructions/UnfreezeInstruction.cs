using System;

namespace PokeAByte.Protocol;

/// <summary>
/// Instruct emulator to drop a previously instructed freeze. 
/// </summary>
public struct UnfreezeInstruction: IEmulatorInstruction
{
    public static Metadata Metadata = new(Instructions.UNFREEZE, 0x00);

    public long Address;

    public byte[] GetByteArray()
    {
        var result = new byte[Metadata.HEADER_LENGTH];
        Metadata.CopyTo(result);
        BitConverter.GetBytes(Address).CopyTo(result, 0x08);
        return result;
    }

    public static UnfreezeInstruction FromByteArray(byte[] bytes)
    {
        var instruction = new UnfreezeInstruction
        {
            Address = BitConverter.ToInt64(bytes, 0x08),
        };
        return instruction;
    }
}
