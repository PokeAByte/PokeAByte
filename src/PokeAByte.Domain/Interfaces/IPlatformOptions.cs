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
    /// The default memory ranges PokeAByte will request from the emulator (for certain drivers anyway).
    /// </summary>
    public MemoryAddressBlock[] Ranges { get; }
}
