using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Application;
public class GBA_PlatformOptions : IPlatformOptions
{
    public EndianTypes EndianType { get; } = EndianTypes.BigEndian;
    public uint MemorySize {get;} = 0x03007FFF;

    public MemoryAddressBlock[] Ranges { get; } =
    [
        new("Partial EWRAM 1", 0x00000000, 0x00003FFF),
        new("Partial EWRAM 2", 0x02020000, 0x02020FFF),
        new("Partial EWRAM 3", 0x02021000, 0x02022FFF),
        new("Partial EWRAM 4", 0x02023000, 0x02023FFF),
        new("Partial EWRAM 5", 0x02024000, 0x02027FFF),
        new("Partial EWRAM 6", 0x02030000, 0x02033FFF),
        new("Partial EWRAM 7", 0x02037000, 0x02039999),
        new("Partial EWRAM 8", 0x0203A000, 0x0203AFFF),
        new("Partial EWRAM 9", 0x0203B000, 0x0203BFFF),
        new("Partial EWRAM 10", 0x0203C000, 0x0203CFFF),
        new("Partial EWRAM 11", 0x0203D000, 0x0203DFFF),
        new("IWRAM", 0x03001000, 0x03004FFF),
        new("IWRAM", 0x03005000, 0x03005FFF),
        new("IWRAM", 0x03006000, 0x03006FFF),
        new("IWRAM", 0x03007000, 0x03007FFF)
    ];
}

