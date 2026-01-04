using System;

namespace PokeAByte.Protocol;

/// <summary>
/// Instruct emulator to freeze a byte sequence in memory.
/// </summary>
public struct FreezeInstruction: IEmulatorInstruction
{
    public static Metadata Metadata = new(Instructions.FREEZE, 0x00);

    public long Address;

    public int Length;

    public byte[] Data;

    public byte[] GetByteArray()
    {
        var result = new byte[Metadata.HEADER_LENGTH + Data.Length];
        Metadata.CopyTo(result);
        BitConverter.GetBytes(Address).CopyTo(result, Metadata.LENGTH);
        BitConverter.GetBytes(Length).CopyTo(result, Metadata.LENGTH + sizeof(long));
        Data.CopyTo(result, Metadata.HEADER_LENGTH);
        return result;
    }

    public static FreezeInstruction FromByteArray(byte[] bytes)
    {
        var startingAddress = BitConverter.ToInt64(bytes, Metadata.LENGTH);
        var dataLength = BitConverter.ToInt32(bytes, Metadata.LENGTH + sizeof(long));
        var instruction = new FreezeInstruction
        {
            Address = startingAddress,
            Length = dataLength,
            Data = new byte[dataLength],
        };
        Buffer.BlockCopy(bytes, Metadata.HEADER_LENGTH, instruction.Data, 0, dataLength);
        return instruction;
    }
}
