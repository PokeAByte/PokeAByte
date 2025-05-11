using System;
using System.Runtime.InteropServices;

namespace PokeAByte.Protocol;

[StructLayout(LayoutKind.Explicit)]
public struct SetupInstruction
{
    [FieldOffset(0)]
    public Metadata Metadata = new Metadata(Instructions.SETUP, 0x00);

    [FieldOffset(8)]
    public int BlockCount;

    [FieldOffset(32)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public ReadBlock[] Data;

    public SetupInstruction(ReadBlock[] data)
    {
        BlockCount = data.Length;
        Data = new ReadBlock[128];
        data.CopyTo(Data, 0);
    }

    public byte[] GetByteArray()
    {
        int size = Marshal.SizeOf(this);
        byte[] bytes = new byte[size];

        IntPtr pointer = IntPtr.Zero;
        try
        {
            pointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, pointer, true);
            Marshal.Copy(pointer, bytes, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
        return bytes;
    }

    public static SetupInstruction FromByteArray(byte[] bytes)
    {
        SetupInstruction instruction = new SetupInstruction();

        int size = Marshal.SizeOf(instruction);
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            instruction = (SetupInstruction)Marshal.PtrToStructure(ptr, instruction.GetType());
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return instruction;
    }
}

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