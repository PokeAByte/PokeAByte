using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Plantforms;

public class NDS_PlatformOptions : IPlatformOptions
{
    /// <inheritdoc/>
    public EndianTypes EndianType { get; } = EndianTypes.BigEndian;
    
    /// <inheritdoc/>
    public uint MemorySize => 0x2400000;

    /// <inheritdoc/>
    public MemoryAddressBlock[] Ranges { get; } = [
        new MemoryAddressBlock("Main RAM", 0x2000000, 0x400000),
    ];
}
