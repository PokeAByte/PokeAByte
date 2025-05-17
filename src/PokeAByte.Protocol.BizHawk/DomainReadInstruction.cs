namespace PokeAByte.Protocol.BizHawk;

/// <summary>
/// Holds all the details needed to read a memory block from Bizhawk. See also <see cref="ReadBlock" />.
/// </summary>
internal struct DomainReadInstruction
{
    /// <summary>
    /// The identifier / name Bizhawk uses for the memory domain.
    /// </summary>
    internal string Domain;

    /// <summary>
    /// Address to start reading from, relative to the memory domain (first address in the domain is 0x0000).
    /// </summary>
    internal long RelativeStart;

    /// <summary>
    /// Address to read to, relative to the memory domain.
    /// </summary>
    internal long RelativeEnd;

    /// <summary>
    /// Position of the block in the MFF (first byte).
    /// </summary>
    internal uint TransferPosition;
}
