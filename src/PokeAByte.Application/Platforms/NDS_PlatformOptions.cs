using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Application;

public class NDS_PlatformOptions : IPlatformOptions
{
    public EndianTypes EndianType { get; } = EndianTypes.BigEndian;
    public uint MemorySize => 0x2400000;

    public MemoryAddressBlock[] Ranges { get; } = [];
}
