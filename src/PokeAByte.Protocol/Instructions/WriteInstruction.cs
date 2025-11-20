using System;
using System.Runtime.InteropServices;

namespace PokeAByte.Protocol;

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct WriteInstruction
{
    public static Metadata Metadata = new(Instructions.WRITE, 0x00);
    [FieldOffset(0x08)]
    public long Address;
    [FieldOffset(0x0F)]
    public int Length;
    [FieldOffset(32)]
    public byte[] Data;

    public byte[] GetByteArray()
    {
        var result = new byte[Metadata.HEADER_LENGTH + Data.Length];
        Metadata.CopyTo(result);
        BitConverter.GetBytes(Address).CopyTo(result, Metadata.LENGTH);
        BitConverter.GetBytes(Length).CopyTo(result, Metadata.LENGTH + 0x08);
        Data.CopyTo(result, Metadata.HEADER_LENGTH);
        return result;
    }

    public static WriteInstruction FromByteArray(byte[] bytes)
    {
        var startingAddress = BitConverter.ToInt64(bytes, Metadata.LENGTH + 0);
        var dataLength = BitConverter.ToInt32(bytes, Metadata.LENGTH + 8);
        var instruction = new WriteInstruction
        {
            Address = startingAddress,
            Length = dataLength,
            Data = new byte[dataLength],
        };
        Buffer.BlockCopy(bytes, Metadata.HEADER_LENGTH, instruction.Data, 0, dataLength);
        return instruction;
    }
}
