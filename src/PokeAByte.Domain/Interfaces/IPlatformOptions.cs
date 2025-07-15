namespace PokeAByte.Domain.Interfaces;

/// <summary>
/// Interface for defining options for an emulated platform (console).
/// </summary>
public interface IPlatformOptions
{
    /// <summary>
    /// The endianness of the emulated platform.
    /// </summary>
    public EndianTypes EndianType { get; }

    /// <summary>
    /// The default memory ranges Poke-A-Byte will request from the emulator (if the driver supports that).
    /// </summary>
    public MemoryAddressBlock[] Ranges { get; }

    /// <summary>
    /// The total system memory size of the platform.
    /// </summary>
    uint MemorySize { get; }
}
