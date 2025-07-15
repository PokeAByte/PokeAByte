namespace PokeAByte.Protocol.BizHawk.PlatformData;

internal static class PlatformConstants
{
    internal static readonly PlatformEntry[] Platforms =
    [
        new PlatformEntry(
            "NES",
            [ new DomainLayout("RAM", 0x00, 0x800) ]
        ),
        new PlatformEntry(
            "SNES",
            [new DomainLayout("WRAM",0x7E0000, 0x10000)]
        ),
        new PlatformEntry(
            "GB",
            [
                new DomainLayout("WRAM", 0xC000, 0x2000),
                new DomainLayout("VRAM", 0x8000, 0x1FFF),
                new DomainLayout("HRAM", 0xFF80, 0x7E)
            ]
        ),
        new PlatformEntry(
            "GBC",
            [
                new DomainLayout("WRAM", 0xC000, 0x2000),
                new DomainLayout("VRAM", 0x8000, 0x1FFF),
                new DomainLayout("HRAM", 0xFF80, 0x7E),
            ]
        ),
        new PlatformEntry(
            "GBA",
            [
                new DomainLayout("EWRAM", 0x02000000, 0x00040000),
                new DomainLayout("IWRAM", 0x03000000, 0x00008000),
            ]
        ),
        new PlatformEntry(
            "NDS",
            [ new DomainLayout("Main RAM", 0x2000000, 0x400000) ],
            15
        )
    ];
}