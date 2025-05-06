using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Application;

public class SNES_PlatformOptions : IPlatformOptions
{
    public EndianTypes EndianType { get; } = EndianTypes.LittleEndian;
    public uint MemorySize {get;} = 0x7FFFFF;

    // Implemented the full WRAM range for the SNES. Tested with my Super Mario RPG Mapper and the BS Zelda Mapper file. -Jesco
    public MemoryAddressBlock[] Ranges { get; } =
    [
        new("?", 0x003000, 0x003112),
        new("?", 0x400000, 0x403EFF),
        new("?", 0x7E0000, 0x7E3A98),
        new("?", 0x7E3A99, 0x7E7531),
        new("?", 0x7E7532, 0x7EAFCA),
        new("?", 0x7EAFCB, 0x7EEA63),
        new("?", 0x7EEA64, 0x7F24FC),
        new("?", 0x7F24FD, 0x7F5F95),
        new("?", 0x7F5F96, 0x7F9A2E),
        new("?", 0x7F9A2F, 0x7FD4C7),
        new("?", 0x7FD4C8, 0x7FFFFF),
    ];
}

