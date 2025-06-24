using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Plantforms;

public class GBA_PlatformOptions : IPlatformOptions
{
    /// <inheritdoc/>
    public EndianTypes EndianType { get; } = EndianTypes.BigEndian;

    /// <inheritdoc/>
    public uint MemorySize { get; } = 0x03007FFF;

    /// <inheritdoc/>
    public MemoryAddressBlock[] Ranges { get; } =
    [
        new ("Partial EWRAM 1", 0x00000000, 0x00003FFF),
        new ("Partial EWRAM 2", 0x2022000, 0x2023FFF),
        new ("Partial EWRAM 3", 0x2024000, 0x2025FFF),
        new ("Partial EWRAM 4", 0x2026000, 0x2027FFF),
        new ("Partial EWRAM 5", 0x2028000, 0x2029FFF),
        new ("Partial EWRAM 6", 0x202A000, 0x202BFFF),
        new ("Partial EWRAM 7", 0x202C000, 0x202DFFF),
        new ("Partial EWRAM 8", 0x202E000, 0x202FFFF),
        new ("Partial EWRAM 9", 0x2030000, 0x2031FFF),
        new ("Partial EWRAM 10", 0x2032000, 0x2033FFF),
        new ("Partial EWRAM 11", 0x2034000, 0x2035FFF),
        new ("Partial EWRAM 12", 0x2036000, 0x2037FFF),
        new ("Partial EWRAM 13", 0x2038000, 0x2039FFF),
        new ("Partial EWRAM 14", 0x203A000, 0x203BFFF),
        new ("Partial EWRAM 15", 0x203C000, 0x203DFFF),
        new ("Partial EWRAM 16", 0x203E000, 0x203FFFF),
        new ("IWRAM", 0x3000000, 0x3001FFF),
        new ("IWRAM", 0x3002000, 0x3003FFF),
        new ("IWRAM", 0x3004000, 0x3005FFF),
        new ("IWRAM", 0x3006000, 0x3007FFF),
    ];
}

