using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Application;
public class GBC_PlatformOptions : IPlatformOptions
{
    public EndianTypes EndianType { get; } = EndianTypes.LittleEndian;
    public uint MemorySize {get;} = 0x15FFF;
    public MemoryAddressBlock[] Ranges { get; } =
    [
        new("Header", 0x0000, 0x00FF),
        new("VRAM", 0x8000, 0x9FFF),
        new("External RAM (Part 1)", 0xA000, 0xAFFF),
        new("External RAM (Part 2)", 0xB000, 0xBFFF),
        new("Work RAM (Bank 0)", 0xC000, 0xCFFF),
        new("Work RAM (Bank 1)", 0xD000, 0xDFFF),
        new("I/O Registers", 0xFF00, 0xFF7F),
        new("High RAM", 0xFF80, 0xFFFE),
        // RAM Banks 0x02:D000 to 0x07:D000 for GBC
        new("Work RAM (Bank 2)", 0x10000, 0x10FFF),
        new("Work RAM (Bank 3)", 0x11000, 0x11FFF),
        new("Work RAM (Bank 6)", 0x14000, 0x14FFF),
        new("Work RAM (Bank 7)", 0x15000, 0x15FFF)
    ];
}
