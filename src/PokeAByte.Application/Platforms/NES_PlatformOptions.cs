using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Application;
public class NES_PlatformOptions : IPlatformOptions
{
    public EndianTypes EndianType { get; } = EndianTypes.BigEndian;
    public uint MemorySize { get; } = 0x07FF;
    public MemoryAddressBlock[] Ranges { get; } = [
        new("Block 0", 0x0000, 0x00FF),
        new("Block 1", 0x0100, 0x01FF),
        new("Block 2", 0x0200, 0x02FF),
        new("Block 3", 0x0300, 0x03FF),
        new("Block 4", 0x0400, 0x04FF),
        new("Block 5", 0x0500, 0x05FF),
        new("Block 6", 0x0600, 0x06FF),
        new("Block 7", 0x0700, 0x07FF),
    ];
}

