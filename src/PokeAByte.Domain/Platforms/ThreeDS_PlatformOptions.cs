using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.Plantforms;

public class ThreeDS_PlatformOptions : IPlatformOptions
{
    /// <inheritdoc/>
    public EndianTypes EndianType { get; } = EndianTypes.LittleEndian;

    /// <inheritdoc/>
    public uint MemorySize => (uint)Ranges.Sum(y => y.EndingAddress - y.EndingAddress);

    /// <inheritdoc/>
    public MemoryAddressBlock[] Ranges { get; } = [
        new MemoryAddressBlock("ARM11_BootRom", 0x00000000, 0x00010000),
        new MemoryAddressBlock("ARM11_VRAM"   , 0x18000000, 0x18000000 + 0x00600000),
        new MemoryAddressBlock("ARM11_WRAM"   , 0x1FF80000, 0x1FF80000 + 0x00080000),
        new MemoryAddressBlock("ARM11_FCRAM"  , 0x20000000, 0x20000000 + 0x08000000),
        new MemoryAddressBlock("ARM9_IO"      , 0x10000000, 0x10000000 + 0x08000000),
        new MemoryAddressBlock("ARM9_VRAM"    , 0x18000000, 0x18000000 + 0x00600000),
        new MemoryAddressBlock("ARM9_DSP"     , 0x1FF00000, 0x1FF00000 + 0x00080000),
        new MemoryAddressBlock("ARM9_WRAM"    , 0x1FF80000, 0x1FF80000 + 0x00080000),
        new MemoryAddressBlock("ARM9_FCRAM"   , 0x20000000, 0x20000000 + 0x00080000),
    ];
}