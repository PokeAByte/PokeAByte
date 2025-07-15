using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Plantforms;

/* ===== PlayStation ===== */
/* http://www.raphnet.net/electronique/psx_adaptor/Playstation.txt */
/* https://problemkaputt.de/psx-spx.htm */
public class PSX_PlatformOptions : IPlatformOptions
{
    /// <inheritdoc/>
    public EndianTypes EndianType { get; } = EndianTypes.BigEndian;

    /// <inheritdoc/>
    public uint MemorySize { get; } = 0xbfc7ffff;

    /// <inheritdoc/>
    public MemoryAddressBlock[] Ranges { get; } =
    [
        new("Kernel", 0x00000000, 0x0000ffff),
        new("User Memory", 0x00010000, 0x001fffff),
        new("Parallel Port", 0x1f000000, 0x001fffff),
        new("Scratch Pad", 0x1f800000, 0x1f8003ff),
        new("User Memory", 0x1f801000, 0x1f802fff),
        new("Kernel and User Memory Mirror", 0x80000000, 0x801fffff),
        new("Kernel and User Memory Mirror", 0xa0000000, 0xa01fffff),
        new("BIOS", 0xBFC00000, 0xbfc7ffff)
    ];
}

