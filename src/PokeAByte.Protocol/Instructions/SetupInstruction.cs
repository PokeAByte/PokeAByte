using System;
using System.Runtime.InteropServices;

namespace PokeAByte.Protocol;

[StructLayout(LayoutKind.Explicit)]
public struct SetupInstruction
{
    [FieldOffset(0)]
    public Metadata Metadata = new Metadata(Instructions.SETUP, 0x00);

    /// <summary>
    /// How many <see cref="ReadBlock"/>s there are.
    /// </summary>
    [FieldOffset(8)]
    public int BlockCount;

    /// <summary>
    /// How many frames to skip in between updating the memory mapped file with new data from
    /// the game memory. <br/>
    /// If set to <c>-1</c>, let the server decide. 
    /// </summary>
    [FieldOffset(12)]
    public int FrameSkip;

    /// <summary>
    /// The memory blocks the server should provide in the memory mapped file. This is always a fixed length array
    /// of 128. The proper number of blocks is stored in <see cref="BlockCount"/>.
    /// </summary>
    [FieldOffset(32)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public ReadBlock[] Data;

    public SetupInstruction(ReadBlock[] data, int frameSkip = -1)
    {
        BlockCount = data.Length;
        FrameSkip = frameSkip;
        Data = new ReadBlock[128];
        data.CopyTo(Data, 0);
    }

    public readonly byte[] GetByteArray()
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
        SetupInstruction instruction = new();

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
