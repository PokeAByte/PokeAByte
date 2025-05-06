using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Application;

public class GB_PlatformOptions : IPlatformOptions
{
    public EndianTypes EndianType { get; } = EndianTypes.LittleEndian;
    public uint MemorySize {get;} = 0xFFFE;

    public MemoryAddressBlock[] Ranges { get; } =
    [
        new("Header", 0x0000, 0x00FF),
        new("VRAM", 0x8000, 0x9FFF),
        new("External RAM (Part 1)", 0xA000, 0xAFFF),
        new("External RAM (Part 2)", 0xB000, 0xBFFF),
        new("Work RAM (Bank 0)", 0xC000, 0xCFFF),
        new("Work RAM (Bank 1)", 0xD000, 0xDFFF),
        new("I/O Registers", 0xFF00, 0xFF7F),
        new("High RAM", 0xFF80, 0xFFFE)
    ];
}

