using System;
using System.Runtime.InteropServices;

namespace PokeAByte.Protocol;

public static class Instructions
{
    public const byte NOOP = 0x00;
    public const byte PING = 0x01;
    public const byte SETUP = 0x02;
    public const byte WRITE = 0x03;
}

public static class SharedConstants
{
    public const string MemoryMappedFileName = "EDPS_MemoryData.bin";
}

/// <summary>
/// Packet metadata. The first 8 byte of the 32 byte header.
/// </summary>
/// <remarks>
/// 5 bytes are reserved for future extensions.
/// </remarks>
[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct Metadata
{
    public const int HEADER_LENGTH = 32;
    public const byte LENGTH = 8;
    /// <summary>
    /// Emulator interface protocol version. <br/>
    /// Byte 0 of the metadata.
    /// </summary>
    [FieldOffset(0)]
    public byte ProtocolVersion = 0x01;

    /// <summary>
    /// The instruction being to the emulator or to which the packet is a response to. <br/>
    /// Byte 4 of the metadata.
    /// </summary>
    [FieldOffset(4)]
    public byte Instruction;

    /// <summary>
    /// Whether the current packet is a request to the emulator (0) or response to the client (1). <br/>
    /// Byte 6 of the metadata.
    /// </summary>
    [FieldOffset(6)]
    public byte IsResponse;

    public void CopyTo(byte[] destination)
    {
        if (destination.Length < LENGTH)
        {
            throw new ArgumentException($"Can not write metadata into byte array smaller than 8");
        }
        destination[0] = ProtocolVersion;
        destination[4] = Instruction;
        destination[5] = IsResponse;
    }

    public Metadata(byte instruction, byte isResponse)
    {
        Instruction = instruction;
        IsResponse = isResponse;
    }
}
